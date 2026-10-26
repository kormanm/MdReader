/* Diagram text arrives as data, never as executable HTML or JavaScript. */
const container = document.getElementById('diagram');
const toolbar = document.getElementById('toolbar');
const zoomLevel = document.getElementById('zoom-level');
let svgElement;
let naturalWidth = 0, naturalHeight = 0, fitScale = 1, zoom = 1;
let hostHeight = 800;
let drag;
mermaid.initialize({
    startOnLoad: false,
    securityLevel: 'strict',
    suppressErrorRendering: true,
    secure: ['secure', 'securityLevel', 'startOnLoad', 'maxTextSize', 'maxEdges', 'suppressErrorRendering'],
    flowchart: { useMaxWidth: false }
});

async function renderDiagram(source) {
    svgElement = undefined;
    toolbar.hidden = true;
    container.classList.remove('ready');
    try {
        const { svg } = await mermaid.render('workflow', source);
        container.innerHTML = svg;
        svgElement = container.querySelector('svg');
        const bounds = svgElement.viewBox.baseVal;
        naturalWidth = bounds.width;
        naturalHeight = bounds.height;
        if (!(naturalWidth > 0 && naturalHeight > 0)) throw new Error('Diagram has no visible bounds.');
        toolbar.hidden = false;
        container.classList.add('ready');
        zoom = 1;
        layout(true);
    } catch (error) {
        svgElement = undefined;
        container.replaceChildren();
        const message = document.createElement('p');
        message.textContent = 'Unable to render Mermaid diagram: ' + error.message;
        const code = document.createElement('pre');
        code.textContent = source;
        container.append(message, code);
        container.style.height = '200px';
    }
    reportHeight();
}

function reportHeight() {
    window.chrome.webview.postMessage(Math.ceil(container.getBoundingClientRect().bottom + 8));
}

// Keep a bounded viewport: zoom changes the SVG, not the size of the document block.
function layout(resetScroll = false) {
    if (!svgElement) return;
    const width = Math.max(1, container.clientWidth - 2);
    const maxHeight = Math.max(120, Math.min(600, hostHeight * 0.6 - 50));
    fitScale = Math.min(1, width / naturalWidth, maxHeight / naturalHeight);
    container.style.height = `${Math.ceil(Math.max(120, naturalHeight * fitScale + 2))}px`;
    applyZoom();
    if (resetScroll) container.scrollTo(0, 0);
    reportHeight();
}

function applyZoom() {
    if (!svgElement) return;
    svgElement.style.width = `${naturalWidth * fitScale * zoom}px`;
    svgElement.style.height = `${naturalHeight * fitScale * zoom}px`;
    zoomLevel.value = `${Math.round(zoom * 100)}%`;
    document.getElementById('zoom-out').disabled = zoom <= 0.25;
    document.getElementById('zoom-in').disabled = zoom >= 8;
}

function changeZoom(multiplier) {
    const previous = zoom;
    zoom = Math.max(0.25, Math.min(8, zoom * multiplier));
    const centerX = container.scrollLeft + container.clientWidth / 2;
    const centerY = container.scrollTop + container.clientHeight / 2;
    applyZoom();
    container.scrollLeft = centerX * zoom / previous - container.clientWidth / 2;
    container.scrollTop = centerY * zoom / previous - container.clientHeight / 2;
}

document.getElementById('zoom-in').addEventListener('click', () => changeZoom(1.25));
document.getElementById('zoom-out').addEventListener('click', () => changeZoom(0.8));
document.getElementById('fit').addEventListener('click', () => { zoom = 1; layout(true); });
container.addEventListener('pointerdown', event => {
    // Leave native scrollbar interaction alone.
    const bounds = container.getBoundingClientRect();
    if (!svgElement || event.button !== 0 ||
        event.clientX - bounds.left >= container.clientWidth ||
        event.clientY - bounds.top >= container.clientHeight) return;
    drag = { x: event.clientX, y: event.clientY, left: container.scrollLeft, top: container.scrollTop };
    container.setPointerCapture(event.pointerId);
    container.classList.add('dragging');
    event.preventDefault();
});
container.addEventListener('pointermove', event => {
    if (!drag) return;
    container.scrollLeft = drag.left + drag.x - event.clientX;
    container.scrollTop = drag.top + drag.y - event.clientY;
});
function stopDrag() { drag = undefined; container.classList.remove('dragging'); }
container.addEventListener('pointerup', stopDrag);
container.addEventListener('pointercancel', stopDrag);
container.addEventListener('lostpointercapture', stopDrag);
window.chrome.webview.addEventListener('message', event => {
    if (typeof event.data === 'string') renderDiagram(event.data);
    else if (event.data.type === 'viewport') {
        hostHeight = event.data.height;
        layout();
    }
});
// SVG stays vector-sharp on resize; recompute fit without reparsing the diagram.
new ResizeObserver(() => layout()).observe(document.body);
