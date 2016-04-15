﻿using System;
using System.Diagnostics;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public class GoogleAction : IStandaloneAction, IAction<string>
    {
        public void RunAction()
        {
            Process.Start("https://www.google.com/");
        }

        public void RunAction(string search)
        {
            Process.Start("https://www.google.com/search?q=" + Uri.EscapeUriString(search));
        }

        public string Name => "Google search";
        public string BoostIdentifier => Name;

        public object GetDetails()
        {
            return Name;
        }

        public IIconResolver GetIconResolver()
        {
            return new EmptyIconResolver();
        }
    }
}