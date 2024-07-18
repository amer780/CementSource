
#if MELONLOADER
using CementGB.Mod.Utilities;
using MelonLoader;
using System.Reflection;
using LogicUI.FancyTextRendering;
#endif
using UnityEngine;

namespace CementGB.Mod.CementMenu;

#if MELONLOADER
[RegisterTypeInIl2Cpp]
#endif
public class SummaryMarkdownController : MonoBehaviour
{
#if MELONLOADER
    public SummaryMarkdownController(IntPtr ptr) : base(ptr) { }
    internal const string embeddedSummaryPath = "CementMod.Assets.summary.md";
    internal static string? SummaryFileContent => FileUtilities.ReadEmbeddedText(Assembly.GetExecutingAssembly(), embeddedSummaryPath);
#endif

    private void Start()
    {
#if MELONLOADER
        var mdRenderer = GetComponent<MarkdownRenderer>();
        if (SummaryFileContent != null) mdRenderer.Source = SummaryFileContent;
#endif
    }
}