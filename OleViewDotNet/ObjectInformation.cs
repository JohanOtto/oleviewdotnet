﻿//    This file is part of OleViewDotNet.
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    /// <summary>
    /// Form to display basic information about an object
    /// </summary>
    public partial class ObjectInformation : DockContent
    {
        private Object m_pObject;
        private Dictionary<string, string> m_properties;
        private COMInterfaceEntry[] m_interfaces;        
        private string m_objName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objName">Descriptive name of the object</param>
        /// <param name="pObject">Managed wrapper to the object</param>
        /// <param name="properties">List of textual properties to display</param>
        /// <param name="interfaces">List of available interfaces</param>
        public ObjectInformation(string objName, Object pObject, Dictionary<string, string> properties, COMInterfaceEntry[] interfaces)
        {
            ObjectCache.Add(objName, pObject, interfaces);
            m_pObject = pObject;
            m_properties = properties;
            m_interfaces = interfaces;            
            m_objName = objName;
            InitializeComponent();         
        }

        /// <summary>
        /// Load the textual properties into a list box
        /// </summary>
        private void LoadProperties()
        {
            listViewProperties.Columns.Add("Key");
            listViewProperties.Columns.Add("Value");

            foreach (KeyValuePair<string, string> pair in m_properties)
            {
                ListViewItem item = listViewProperties.Items.Add(pair.Key);
                item.SubItems.Add(pair.Value);
            }

            try
            {
                /* Also add IObjectSafety information if available */
                IObjectSafety objSafety = (IObjectSafety)m_pObject;
                uint supportedOptions;
                uint enabledOptions;
                Guid iid = COMInterfaceEntry.IID_IDispatch;

                objSafety.GetInterfaceSafetyOptions(ref iid, out supportedOptions, out enabledOptions);
                for (int i = 0; i < 4; i++)
                {
                    int val = 1 << i;
                    if ((val & supportedOptions) != 0)
                    {
                        ListViewItem item = listViewProperties.Items.Add(Enum.GetName(typeof(ObjectSafetyFlags), val));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        /// <summary>
        /// Load interface list into list box
        /// </summary>
        private void LoadInterfaces()
        {
            listViewInterfaces.Columns.Add("Name");
            listViewInterfaces.Columns.Add("IID");
            listViewInterfaces.Columns.Add("Viewer");

            foreach (COMInterfaceEntry ent in m_interfaces)
            {
                ListViewItem item = listViewInterfaces.Items.Add(ent.Name);
                item.Tag = ent;
                item.SubItems.Add(ent.Iid.ToString("B"));

                InterfaceViewers.ITypeViewerFactory factory = InterfaceViewers.InterfaceViewers.GetInterfaceViewer(ent.Iid);
                if (factory != null)
                {
                    item.SubItems.Add("Yes");
                }
                else
                {
                    item.SubItems.Add("No");
                }

                if (ent.IsDispatch)
                {
                    btnDispatch.Enabled = true;
                }
                else if (ent.IsOleControl)
                {
                    btnOleContainer.Enabled = true;
                }
            }

            listViewInterfaces.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewInterfaces.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ObjectInformation_Load(object sender, EventArgs e)
        {
            LoadProperties();
            LoadInterfaces();
            TabText = m_objName;
        }

        private void listViewInterfaces_DoubleClick(object sender, EventArgs e)
        {
            if (listViewInterfaces.SelectedItems.Count > 0)
            {
                COMInterfaceEntry ent = (COMInterfaceEntry)listViewInterfaces.SelectedItems[0].Tag;
                InterfaceViewers.ITypeViewerFactory factory = InterfaceViewers.InterfaceViewers.GetInterfaceViewer(ent.Iid);

                try
                {
                    if (factory != null)
                    {
                        DockContent frm = factory.CreateInstance(m_objName, m_pObject);
                        if ((frm != null) && !frm.IsDisposed)
                        {
                            frm.ShowHint = DockState.Document;
                            frm.Show(this.DockPanel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnOleContainer_Click(object sender, EventArgs e)
        {
            DockContent frm = new ObjectContainer(m_objName, m_pObject);
            if ((frm != null) && !frm.IsDisposed)
            {
                frm.ShowHint = DockState.Document;
                frm.Show(this.DockPanel);
            }
        }

        private void btnDispatch_Click(object sender, EventArgs e)
        {
            DockContent frm = new TypedObjectViewer(m_objName, m_pObject, COMUtilities.GetDispatchTypeInfo(m_pObject)); ;
            if ((frm != null) && !frm.IsDisposed)
            {
                frm.ShowHint = DockState.Document;
                frm.Show(this.DockPanel);
            }
        }

        private void ObjectInformation_FormClosed(object sender, FormClosedEventArgs e)
        {
            ObjectCache.Remove(m_pObject);
        }
    }
}
