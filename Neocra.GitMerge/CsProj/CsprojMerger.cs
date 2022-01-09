using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Neocra.GitMerge.Tools;
using Neocra.GitMerge.Xml;

namespace Neocra.GitMerge.CsProj
{
    public class CsprojMerger : XmlMerger
    {

        public CsprojMerger(ILogger<XmlMerger> logger, XmlDataAccess xmlDataAccess, DiffTools diffTools) : 
            base(logger, xmlDataAccess, diffTools)
        {
        }

        public override string ProviderCode => "csproj";

        protected override bool ResolveConflict(List<XmlDiff> diffsCurrent, List<XmlDiff> diffsOther, XmlDiff diffCurrent, XmlDiff? diffOther)
        {
            if (diffOther == null)
            {
                throw new NotSupportedException();
            }
            
            switch (diffCurrent.Mode, diffOther.Mode)
            {
                case (DiffMode.Update, DiffMode.Update):
                    if (Regex.IsMatch(diffCurrent.ParentPath, @"PackageReference\[[0-9]+\]"))
                    {
                        if (diffCurrent.Value is XAttribute attribute1)
                        {
                            if (diffOther.Value is XAttribute attribute2)
                            {
                                if (attribute1.Name == attribute2.Name
                                    && attribute1.Name == "Version")
                                {
                                    if (attribute1.Value != attribute2.Value)
                                    {
                                        this.logger.LogInformation("Update version");

                                        if (this.IsVersionGreater(attribute1.Value, attribute2.Value))
                                        {
                                            diffsOther.Remove(diffOther);
                                        }
                                        else
                                        {
                                            diffsCurrent.Remove(diffCurrent);
                                        }
                                    
                                        return false;
                                    }

                                    diffsCurrent.Remove(diffCurrent);
                                    return false;
                                }
                            }
                        }
                    }

                    return base.ResolveConflict(diffsCurrent, diffsOther, diffCurrent, diffOther);
            }

            
            return base.ResolveConflict(diffsCurrent, diffsOther, diffCurrent, diffOther);
        }

        private bool IsVersionGreater(string version1, string version2)
        {
            return new Version(version1).CompareTo(new Version(version2)) > 0;
        }
    }
}