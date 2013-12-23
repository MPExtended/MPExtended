#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Runtime;

namespace MPExtended.Libraries.Service.WCF
{
    public class CustomWebHttpBindingElement : WebHttpBindingElement
    {
        private ConfigurationPropertyCollection properties;

        public CustomWebHttpBindingElement() : base()
        {
        }

        public CustomWebHttpBindingElement(string name) : base(name)
        {
        }

        protected override Type BindingElementType
        {
            get { return typeof(CustomWebHttpBinding); }
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            base.OnApplyConfiguration(binding);

            CustomWebHttpBinding customWebHttpBinding = (CustomWebHttpBinding)binding;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection configurationPropertyCollection = base.Properties;
                    this.properties = configurationPropertyCollection;
                }
                return this.properties;
            }
        }
    }

    public class CustomWebHttpBindingCollectionElement : StandardBindingCollectionElement<CustomWebHttpBinding, CustomWebHttpBindingElement>
    {
        protected override Binding GetDefault()
		{
            return new CustomWebHttpBinding();
		}

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CustomWebHttpBindingCollectionElement()
		{
		}
    }
}
