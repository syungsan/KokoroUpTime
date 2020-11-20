using System;
using System.Windows;
using System.Windows.Controls;


namespace KokoroUpTime
{
    public class InputMethodStyleSelector : StyleSelector
    {
        public Style HandWritingInputStyle { get; set; }
        public Style KeyBoardInputStyle { get; set; }

        DataOption dataOption = new DataOption();

        public Style SelectStyle(object item, DependencyObject container,int inputMethod)
        {
            try
            {
                if (this.dataOption.InputMethod == 0)
                    return HandWritingInputStyle; //手書き
                else 
                    return KeyBoardInputStyle;　//キーボード
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("エラーが起きたため手書き入力に切り替えます");

                return HandWritingInputStyle;
            }
        }
    }
}
