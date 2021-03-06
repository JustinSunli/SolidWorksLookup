﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using SldWorksLookup.Properties;
using SldWorksLookup.Model;
using SolidWorks.Interop.swconst;
using System.Diagnostics;
using SolidWorks.Interop.sldworks;

namespace SldWorksLookup
{
    [ComVisible(true)]
    [Title("SolidWorks Lookup")]
    [Description("Lookup SolidWorks Objects 0.0.1")]
    [Icon(typeof(Resource),nameof(Resource.BrowseData_16x))]
    public class AddIn:SwAddInEx
    {
        public override void OnConnect()
        {
            var cmdGroup = CommandManager.AddCommandGroup<Command_e>();
            cmdGroup.CommandClick += CmdGroup_CommandClick;
            //cmdGroup.CommandStateResolve += CmdGroup_CommandStateResolve;
        }

        private void CmdGroup_CommandStateResolve(Command_e spec, Xarial.XCad.UI.Commands.Structures.CommandState state)
        {
            switch (spec)
            {
                case Command_e.Lookup:
                    state.Enabled = true;
                    break;
                case Command_e.CurrentSelection:
                    var seleCount = Application?.Documents?.Active?.Selections.Count;
                    state.Enabled = seleCount.HasValue ? seleCount.Value > 0 : false;
                    break;               
                default:
                    break;
            }
        }

        private void CmdGroup_CommandClick(Command_e spec)
        {
            switch (spec)
            {
                case Command_e.Lookup:

                    SnoopISldWorks();
                    break;

                case Command_e.ActiveDoc:

                    SnoopActiveDoc();
                    break;

                case Command_e.CurrentSelection:

                    SnoopCurrentSelection();
                    break;

                case Command_e.TestFramework:

                    Process.Start(new ProcessStartInfo("https://github.com/weianweigan/SldWorks.TestRunner"));
                    break;
            }
        }

        private void SnoopISldWorks()
        {
            var popWindow = CreatePopupWindow<View.LookupPropertyWindow>();
            popWindow.Control.ISldWorksInit(this.Application);
            popWindow.Show();
        }

        private void SnoopActiveDoc()
        {
            var mdlDoc = Application.Sw.IActiveDoc2;
            if (mdlDoc == null)
            {
                Application.Sw.SendMsgToUser("No ActiveDoc");
                return;
            }
            var docWindow = CreatePopupWindow<View.LookupPropertyWindow>();
            docWindow.Control.MutiInit(new InstanceProperty[] { new InstanceProperty(mdlDoc, typeof(IModelDoc2)) });
            docWindow.Show();
        }

        private void SnoopCurrentSelection()
        {
            var doc = this.Application.Sw.IActiveDoc2;
            if (doc == null)
            {
                Application.Sw.SendMsgToUser("No ActiveDoc");
                return;
            }
            var count = doc.ISelectionManager.GetSelectedObjectCount();
            var ins = new List<InstanceProperty>();
            for (int i = 1; i < count + 1; i++)
            {
                var mark = doc.ISelectionManager.GetSelectedObjectMark(i);
                var obj = doc.ISelectionManager.GetSelectedObject6(i, mark);
                var type = (swSelectType_e)doc.ISelectionManager.GetSelectedObjectType3(i, mark);
                var matchType = SelectTypeMatcherUtil.Match(type);
                if (matchType != null)
                {
                    var insPro = new InstanceProperty(obj, matchType);
                    ins.Add(insPro);
                }
                else
                {
                    Application.Sw.SendMsgToUser($"{type} Cannot match a SolidWorks Interface");
                }
            }
            var selPpopWindow = CreatePopupWindow<View.LookupPropertyWindow>();
            selPpopWindow.Control.MutiInit(ins);
            selPpopWindow.Show();
        }
    }
}
