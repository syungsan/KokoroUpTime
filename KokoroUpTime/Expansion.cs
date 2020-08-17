using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Windows.Media;

namespace Expansion
{
    // データテンプレート内の子要素群にアクセスする
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<T> GetChildren<T>(this DependencyObject p_element, Func<T, bool> p_func = null) where T : UIElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(p_element); i++)
            {
                UIElement child = VisualTreeHelper.GetChild(p_element, i) as FrameworkElement;
                if (child == null)
                {
                    continue;
                }

                if (child is T)
                {
                    var t = (T)child;
                    if (p_func != null && !p_func(t))
                    {
                        continue;
                    }

                    yield return t;
                }
                else
                {
                    foreach (var c in child.GetChildren(p_func))
                    {
                        yield return c;
                    }
                }
            }
        }
    }

    // foreach ステートメントで、インデックス付きで列挙したい
    public static partial class TupleEnumerable
    {
        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            IEnumerable<(T item, int index)> impl()
            {
                var i = 0;
                foreach (var item in source)
                {
                    yield return (item, i);
                    ++i;
                }
            }
            return impl();
        }
    }
}
