using System;
using System.Windows;
using System.Windows.Controls;


namespace KokoroUpTime
{
    public class InputMethodStyleSelector : StyleSelector
    {
        public Style HandWritingStyle { get; set; }
        public Style KeyBoardStyle { get; set; }

        DataOption dataOption = new DataOption();

        public override Style SelectStyle(object item, DependencyObject container)
        {
            try
            {
                if (this.dataOption.InputMethod == 0)
                    return HandWritingStyle; //手書き
                else
                    return KeyBoardStyle;　//キーボード
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("エラーが起きたため手書き入力に切り替えます");

                return HandWritingStyle;
            }
        }
    }
}
