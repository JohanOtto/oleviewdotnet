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

using System.Management.Automation;

namespace OleViewDotNet
{
    [Cmdlet(VerbsCommon.Get, "ComRegistry")]
    class COMRegistryCmdlet : Cmdlet
    {
        protected override void ProcessRecord()
        {
            COMRegistry reg = Program.GetCOMRegistry();
            if (reg != null)
            {
                WriteObject(reg);
            }
        }
    }
}
