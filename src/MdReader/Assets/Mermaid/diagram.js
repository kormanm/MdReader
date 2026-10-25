/* Diagram text arrives as data, never as executable HTML or JavaScript. */
const container = document.getElementById('diagram');
mermaid.initialize({
    startOnLoad: false,
    securityLevel: 'strict',
    suppressErrorRendering: true,
    secure: ['secure', 'securityLevel', 'startOnLoad', 'maxTextSize', 'maxEdges', 'suppressErrorRendering'],
    flowchart: { useMaxWidth: false }
});

async function renderDiagram(source) {
    try {
        const { svg } = await mermaid.render('workflow', source);
        container.innerHTML = svg;
    } catch (error) {
        container.replaceChildren();
        const message = document.createElement('p');
        message.textContent = 'Unable to render Mermaid diagram: ' + error.message;
        const code = document.createElement('pre');
        code.textContent = source;
        container.append(message, code);
    }
    reportHeight();
}

function reportHeight() {
    window.chrome.webview.postMessage(Math.ceil(container.getBoundingClientRect().height + 24));
}
window.chrome.webview.addEventListener('message', event => renderDiagram(event.data));
new ResizeObserver(reportHeight).observe(container);
