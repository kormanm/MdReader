'use strict';

const { test } = require('node:test');
const assert = require('node:assert/strict');
const { KEEP_BUILDS, planDeletions, pruneArtifacts } = require('./prune-artifacts.cjs');
const rules = [{ pattern: '^output$', family: 'output' }];
const run = (id, status = 'completed', workflow_id = 7) => ({ id, status, workflow_id });
const artifact = (id, runId = id, name = 'output', extra = {}) => ({
  id, name, workflow_run: { id: runId }, created_at: new Date(id * 1000).toISOString(),
  expired: false, size_in_bytes: 10, ...extra
});
const runsFor = artifacts => new Map(artifacts.map(a => [a.workflow_run.id, run(a.workflow_run.id)]));

test('keeps current plus two previous completed builds, including an empty repository', () => {
  assert.equal(KEEP_BUILDS, 3);
  assert.deepEqual(planDeletions([], new Map(), rules), []);
  for (let count = 1; count <= 6; count++) {
    const items = Array.from({ length: count }, (_, i) => artifact(i + 1));
    assert.deepEqual(planDeletions(items, runsFor(items), rules).map(a => a.id),
      Array.from({ length: Math.max(0, count - 3) }, (_, i) => Math.max(0, count - 3) - i));
  }
});

test('preserves every Docker record in each of the newest three build sets', () => {
  const items = [1, 2, 3, 4].flatMap(id => [artifact(id * 10, id, 'record-a.dockerbuild'),
    artifact(id * 10 + 1, id, 'record-b.dockerbuild')]);
  const docker = [{ pattern: '\\.dockerbuild$', family: 'docker', perWorkflow: true }];
  assert.deepEqual(planDeletions(items, runsFor(items), docker).map(a => a.id), [10, 11]);
});

test('versioned installer names share a family; other families and workflows stay separate', () => {
  const items = [1, 2, 3, 4].map(id => artifact(id, id, 'setup-' + id));
  items.push(artifact(20, 20, 'other'), artifact(30, 30, 'record.dockerbuild'));
  const configured = [{ pattern: '^setup-', family: 'installer' },
    { pattern: '\\.dockerbuild$', family: 'docker', perWorkflow: true }];
  assert.deepEqual(planDeletions(items, runsFor(items), configured).map(a => a.id), [1]);
  const records = [1, 2, 3, 4, 5, 6].map(id => artifact(id, id, 'record.dockerbuild'));
  const runs = new Map(records.map(a => [a.id, run(a.id, 'completed', a.id <= 3 ? 1 : 2)]));
  assert.deepEqual(planDeletions(records, runs, configured), []);
});

test('does not count or delete active, unavailable, expired, or unrelated artifacts', () => {
  const items = [1, 2, 3, 4, 5, 6, 7].map(id => artifact(id));
  items.push(artifact(8, 8, 'unrelated'), artifact(9, 9, 'output', { expired: true }));
  const runs = runsFor(items);
  runs.set(4, run(4, 'in_progress'));
  runs.set(5, run(5, 'queued'));
  runs.set(6, run(6, 'waiting'));
  runs.delete(7);
  assert.deepEqual(planDeletions(items, runs, rules), []);
});

test('sorts API results deterministically instead of relying on response order', () => {
  const items = [4, 1, 3, 2].map(id => artifact(id, id, 'output', { created_at: '2026-01-01T00:00:00Z' }));
  assert.deepEqual(planDeletions(items, runsFor(items), rules).map(a => a.id), [1]);
});

test('rejects invalid matching metadata before making a deletion plan', () => {
  const items = [1, 2, 3, 4].map(id => artifact(id));
  items[0].created_at = 'invalid';
  assert.throws(() => planDeletions(items, runsFor(items), rules), /refusing cleanup/);
});

function harness(items, overrides = {}) {
  const deleted = [];
  const calls = [];
  const summary = { addHeading() { return this; }, addRaw() { return this; }, async write() {} };
  const github = {
    async paginate(endpoint, options) {
      assert.equal(endpoint, github.rest.actions.listArtifactsForRepo);
      assert.equal(options.per_page, 100);
      return items;
    },
    rest: { actions: {
      listArtifactsForRepo() {},
      async getWorkflowRun({ run_id }) {
        calls.push(run_id);
        return { data: run(run_id) };
      },
      async deleteArtifact({ artifact_id }) { deleted.push(artifact_id); },
      ...overrides
    } }
  };
  return { github, context: { repo: { owner: 'owner', repo: 'repo' } },
    core: { info() {}, warning() {}, summary }, rules, deleted, calls };
}

test('paginates discovery and removes only older completed builds', async () => {
  const h = harness([1, 2, 3, 4, 5].map(id => artifact(id)));
  const result = await pruneArtifacts({ ...h, dryRun: false });
  assert.deepEqual(h.deleted, [2, 1]);
  assert.equal(result.bytes, 20);
  assert.equal(h.calls.filter(id => id === 1).length, 2);
});

test('dry run is the default and never calls the delete endpoint', async () => {
  const h = harness([1, 2, 3, 4].map(id => artifact(id)));
  const result = await pruneArtifacts(h);
  assert.equal(result.removed, 1);
  assert.deepEqual(h.deleted, []);
});

test('protects a completed run that starts rerunning during cleanup', async () => {
  let reads = 0;
  const h = harness([1, 2, 3, 4].map(id => artifact(id)), {
    async getWorkflowRun({ run_id }) {
      return { data: run(run_id, run_id === 1 && ++reads > 1 ? 'in_progress' : 'completed') };
    }
  });
  await pruneArtifacts({ ...h, dryRun: false });
  assert.deepEqual(h.deleted, []);
});

test('discovery failures prevent all deletions; missing runs are protected', async () => {
  const h = harness([1, 2, 3, 4].map(id => artifact(id)), {
    async getWorkflowRun() { throw Object.assign(new Error('API unavailable'), { status: 403 }); }
  });
  await assert.rejects(pruneArtifacts({ ...h, dryRun: false }), /API unavailable/);
  assert.deepEqual(h.deleted, []);
  const missing = harness([1, 2, 3, 4].map(id => artifact(id)), {
    async getWorkflowRun() { throw Object.assign(new Error('missing'), { status: 404 }); }
  });
  await pruneArtifacts({ ...missing, dryRun: false });
  assert.deepEqual(missing.deleted, []);
});

test('already removed artifacts are tolerated; permission failures are reported', async () => {
  const items = [1, 2, 3, 4].map(id => artifact(id));
  const missing = harness(items, {
    async deleteArtifact() { throw Object.assign(new Error('missing'), { status: 404 }); }
  });
  await pruneArtifacts({ ...missing, dryRun: false });
  const denied = harness(items, {
    async deleteArtifact() { throw Object.assign(new Error('denied'), { status: 403 }); }
  });
  await assert.rejects(pruneArtifacts({ ...denied, dryRun: false }), /denied/);
});
