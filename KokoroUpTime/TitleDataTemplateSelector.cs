using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace KokoroUpTime
{
    internal class TitleDataTemplateSelector : DataTemplateSelector
    {
            public override DataTemplate SelectTemplate(object item,DependencyObject container)
            {
                var element = container as FrameworkElement;
                var viewModel = item as TitleTemplateData;
                var templateName = "";
                switch (viewModel.TitleTemplate)
                {
                    case "thoughts_and_feeling_title":
                        templateName = "ThoughtsAndFeelingTitleTemplate"; break;


                    case "challenge_time_title":
                        templateName = "ChallengeTimeTitleTemplate"; break;

                    case "how_to_use_item_title":
                        templateName = "HowToUseItemTitleTemplate"; break;
                    default:
                        templateName = "ThoughtsAndFeelingTitleTemplate"; break;
                }    
                    
                return templateName == ""? null
                : element.FindResource(templateName) as DataTemplate;

            }
    }

    public class TitleTemplateData 
    {
        private string _TitleTemplate;
        public string TitleTemplate 
        {
            get { return _TitleTemplate; }

            set
            {
                if (_TitleTemplate == value)
                    return;
                _TitleTemplate = value;
            }
        }

        private string _TitleText;
        public string TitleText
        {
            get { return _TitleText; }

            set
            {
                if (_TitleText == value)
                    return;
                _TitleText = value;
            }
        }

        private string _UpperRowText;
        public string UpperRowText
        {
            get { return _UpperRowText; }

            set
            {
                if (_UpperRowText == value)
                    return;
                _UpperRowText = value;
            }
        }

        private string _RedText;
        public string RedText
            {
            get { return _RedText; }

            set
            {
                if (_RedText == value)
                    return;
                _RedText = value;
            }
        }

        private string _LowerRowText;
        public string LowerRowText 
            {
            get { return _LowerRowText; }

            set
            {
                if (_LowerRowText == value)
                    return;
                _LowerRowText = value;
            }
        }
    }
}
