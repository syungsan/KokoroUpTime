using System.Windows;
using System.Windows.Controls;

namespace KokoroUpTime
{
    public class InputMethodStyleSelector : StyleSelector
    {
        public Style HandWritingInputStyle { get; set; }
        public Style KeyboardInputStyle { get; set; }

        public Style SelectStyle(int inputMethod)
        {
            if (inputMethod == 1)
                return KeyboardInputStyle; //キーボード
            else
                return HandWritingInputStyle; //手書き
        }
    }
}
