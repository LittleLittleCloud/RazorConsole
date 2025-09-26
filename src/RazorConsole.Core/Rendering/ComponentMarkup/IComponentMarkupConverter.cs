using System.Text;
using System.Xml.Linq;

namespace RazorConsole.Core.Rendering.ComponentMarkup;

internal interface IComponentMarkupConverter
{
    bool TryConvert(XElement element, StringBuilder builder);
}
