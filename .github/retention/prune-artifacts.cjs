'use strict';

const KEEP_BUILDS = 3;

function planDeletions(artifacts, runs, rules) {
  const groups = new Map();
  for (const artifact of artifacts) {
    if (artifact.expired) continue;
    const rule = rules.find(item => new RegExp(item.pattern).test(artifact.name));
    if (!rule) continue;
    const run = runs.get(artifact.workflow_run?.id);
    // Missing or active runs are protected, including queued reruns.
    if (!run || run.status !== 'completed') continue;
    const timestamp = Date.parse(artifact.created_at);
    if (!Number.isSafeInteger(artifact.id) || artifact.id <= 0 ||
        !Number.isSafeInteger(run.id) || !Number.isFinite(timestamp) ||
        (rule.perWorkflow && !Number.isSafeInteger(run.workflow_id))) {
      throw new Error('Invalid artifact/run metadata; refusing cleanup.');
    }
    const key = rule.family + (rule.perWorkflow ? ':' + run.workflow_id : '');
    if (!groups.has(key)) groups.set(key, new Map());
    const builds = groups.get(key);
    if (!builds.has(run.id)) builds.set(run.id, { id: run.id, timestamp, artifacts: [] });
    const build = builds.get(run.id);
    build.timestamp = Math.max(build.timestamp, timestamp);
    build.artifacts.push(artifact);
  }

  const deletions = [];
  for (const builds of groups.values()) {
    const newestFirst = [...builds.values()].sort((a, b) => b.timestamp - a.timestamp || b.id - a.id);
    for (const build of newestFirst.slice(KEEP_BUILDS)) deletions.push(...build.artifacts);
  }
  return deletions;
}

async function pruneArtifacts({ github, context, core, rules, dryRun = true }) {
  const repo = context.repo;
  const artifacts = await github.paginate(github.rest.actions.listArtifactsForRepo, {
    ...repo, per_page: 100
  });
  const matching = artifacts.filter(artifact => !artifact.expired &&
    rules.some(rule => new RegExp(rule.pattern).test(artifact.name)));
  const runs = new Map();
  for (const runId of new Set(matching.map(artifact => artifact.workflow_run?.id))) {
    if (!Number.isSafeInteger(runId) || runId <= 0) continue;
    try {
      const { data } = await github.rest.actions.getWorkflowRun({ ...repo, run_id: runId });
      runs.set(runId, data);
    } catch (error) {
      if (error.status !== 404) throw error;
      core.warning('Protecting artifacts of unavailable run ' + runId);
    }
  }

  // Finish discovery and validation before deleting anything.
  const deletions = planDeletions(artifacts, runs, rules);
  const checkedRuns = new Map();
  let removed = 0;
  let bytes = 0;
  for (const artifact of deletions) {
    const runId = artifact.workflow_run.id;
    if (!checkedRuns.has(runId)) {
      try {
        const { data } = await github.rest.actions.getWorkflowRun({ ...repo, run_id: runId });
        checkedRuns.set(runId, data.status === 'completed');
      } catch (error) {
        if (error.status !== 404) throw error;
        checkedRuns.set(runId, false);
      }
    }
    if (!checkedRuns.get(runId)) continue;
    core.info((dryRun ? 'Would delete ' : 'Deleting ') + artifact.name + ' (' + artifact.id + ')');
    if (!dryRun) {
      try {
        await github.rest.actions.deleteArtifact({ ...repo, artifact_id: artifact.id });
      } catch (error) {
        if (error.status !== 404) throw error;
      }
    }
    removed++;
    bytes += artifact.size_in_bytes;
  }
  await core.summary.addHeading('Artifact retention')
    .addRaw('Policy: current completed build + two previous builds per artifact family. Active runs are protected.\n\n')
    .addRaw((dryRun ? 'Dry run: would remove ' : 'Removed ') + removed + ' artifacts (' + bytes + ' bytes).\n')
    .write();
  return { removed, bytes, dryRun };
}

module.exports = { KEEP_BUILDS, planDeletions, pruneArtifacts };
