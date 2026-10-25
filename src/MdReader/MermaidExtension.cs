using System;
using System.Windows.Documents;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Syntax;

namespace MdReader;

/// <summary>Keep the native Markdown viewer and replace only Mermaid code blocks.</summary>
internal sealed class MermaidExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline) { }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is WpfRenderer wpf)
            wpf.ObjectRenderers.Insert(0, new MermaidCodeBlockRenderer());
    }

    private sealed class MermaidCodeBlockRenderer : CodeBlockRenderer
    {
        protected override void Write(WpfRenderer renderer, CodeBlock block)
        {
            if (block is FencedCodeBlock fenced &&
                string.Equals(fenced.Info, "mermaid", StringComparison.OrdinalIgnoreCase))
            {
                renderer.WriteBlock(new BlockUIContainer(new MermaidDiagram(block.Lines.ToString())));
                return;
            }

            base.Write(renderer, block);
        }
    }
}
