using System;
using System.Windows;
using System.Windows.Controls;


namespace KokoroUpTime
{
    public class InputMethodStyleSelector : StyleSelector
    {
        public Style HandWritingInputStyle { get; set; }
        public Style KeyBoardInputStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container/*,int inputMethod*/)
        {
            try
            {
                if (/*inputMethod == 0*/true)
                    return HandWritingInputStyle; //手書き
                ////else 
                //    return KeyBoardInputStyle;　//キーボード
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("エラーが起きたため手書き入力に切り替えます");

                return HandWritingInputStyle;
            }
        }
    }
}
