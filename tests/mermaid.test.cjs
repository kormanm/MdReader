const { test, before, after } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const http = require('node:http');
const { chromium } = require('playwright');

const assets = path.resolve(__dirname, '../src/MdReader/Assets/Mermaid');
let server, browser, origin;
before(async () => {
    server = http.createServer((req, res) => {
        const name = path.basename(new URL(req.url, 'http://localhost').pathname);
        const file = path.join(assets, name);
        if (!fs.existsSync(file)) { res.writeHead(404).end(); return; }
        res.setHeader('Content-Type', name.endsWith('.js') ? 'text/javascript' : 'text/html');
        res.end(fs.readFileSync(file));
    });
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    origin = `http://127.0.0.1:${server.address().port}`;
    browser = await chromium.launch({
        headless: true,
        ...(process.env.CHROMIUM_PATH ? { executablePath: process.env.CHROMIUM_PATH } : {})
    });
});
after(async () => {
    await browser?.close();
    if (server) await new Promise(resolve => server.close(resolve));
});

async function diagramPage() {
    const page = await browser.newPage();
    await page.addInitScript(() => {
        window.chrome = { webview: {
            addEventListener: (_, callback) => window.sendDiagram = source => callback({ data: source }),
            postMessage: height => window.diagramHeight = height
        } };
    });
    await page.goto(`${origin}/diagram.html`);
    await page.waitForFunction(() => typeof window.sendDiagram === 'function');
    return page;
}

test('renders the migration workflow offline with all nodes, edges and multiline labels', async () => {
    const page = await diagramPage();
    const externalRequests = [];
    page.on('request', request => {
        if (!request.url().startsWith(origin)) externalRequests.push(request.url());
    });
    await page.evaluate(source => window.sendDiagram(source),
        fs.readFileSync(path.join(__dirname, 'fixtures/workflow.mmd'), 'utf8'));
    await page.waitForSelector('#diagram svg .node');
    assert.equal(await page.locator('#diagram svg .node').count(), 5);
    assert.equal(await page.locator('#diagram svg path.flowchart-link').count(), 4);
    assert.match(await page.locator('#diagram').innerText(), /legacy code deleted/);
    assert.ok(await page.locator('#diagram br').count() >= 10);
    assert.ok(await page.evaluate(() => window.diagramHeight > 100));
    assert.deepEqual(externalRequests, []);
    await page.close();
});

test('invalid diagrams show source and an error, and the next render can recover', async () => {
    const page = await diagramPage();
    const source = 'flowchart LR\nA[unclosed';
    await page.evaluate(source => window.sendDiagram(source), source);
    await page.waitForSelector('#diagram pre');
    assert.equal(await page.locator('#diagram pre').innerText(), source);
    assert.match(await page.locator('#diagram p').innerText(), /Unable to render/);
    await page.evaluate(() => window.sendDiagram('flowchart LR\nA[Start] --> B[Done]'));
    await page.waitForSelector('#diagram svg .node');
    assert.equal(await page.locator('#diagram svg .node').count(), 2);
    assert.equal(await page.locator('#diagram pre').count(), 0);
    await page.close();
});

test('HTML in diagram labels cannot execute script', async () => {
    const page = await diagramPage();
    await page.evaluate(() => window.sendDiagram(
        '%%{init: {"securityLevel": "loose"}}%%\nflowchart LR\nA["<img src=x onerror=window.diagramInjected=true>"] --> B[Safe]'));
    await page.waitForFunction(() => document.querySelector('#diagram svg, #diagram pre'));
    assert.equal(await page.evaluate(() => window.diagramInjected), undefined);
    assert.equal(await page.locator('#diagram [onerror], #diagram script').count(), 0);
    await page.close();
});
