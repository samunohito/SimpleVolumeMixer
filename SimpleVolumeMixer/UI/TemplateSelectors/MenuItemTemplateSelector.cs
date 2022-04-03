using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.TemplateSelectors;

public class MenuItemTemplateSelector : DataTemplateSelector
{
    public MenuItemTemplateSelector()
    {
        GlyphDataTemplate = null;
        ImageDataTemplate = null;
        PackIconTemplate = null;
    }
    
    public DataTemplate? GlyphDataTemplate { get; set; }

    public DataTemplate? ImageDataTemplate { get; set; }
    
    public DataTemplate? PackIconTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is HamburgerMenuGlyphItem) return GlyphDataTemplate;

        if (item is HamburgerMenuImageItem) return ImageDataTemplate;

        if (item is HamburgerMenuPackIconItem) return PackIconTemplate;

        return base.SelectTemplate(item, container);
    }
}