#region Copyright (C) 2013 MPExtended, Microsoft Corporation
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) Microsoft Corporation, http://www.microsoft.com/
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
//
// Based upon original assumed Public Domain (since a license declaration
// is missing) code from Microsoft Corporation, obtained from
// http://go.microsoft.com/fwlink/?LinkId=150780.
// Additional fixes based on code from Francesco De Vittori, 
// http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/
#endregion

using System;
using System.Xml;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;


namespace MPExtended.Libraries.Service.Compression
{
    //This is the binding element that, when plugged into a custom binding, will enable the compression encoder
    // Removed IPolicyExportExtension as it's not used directly by app.config
    public sealed class CompressionMessageEncodingBindingElement
                        : MessageEncodingBindingElement //BindingElement
    {

        //We will use an inner binding element to store information required for the inner encoder
        MessageEncodingBindingElement innerBindingElement;

        //By default, use the default text encoder as the inner encoder
        public CompressionMessageEncodingBindingElement()
            : this(new TextMessageEncodingBindingElement()) { }

        public CompressionMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement)
        {
            this.innerBindingElement = messageEncoderBindingElement;
        }

        public MessageEncodingBindingElement InnerMessageEncodingBindingElement
        {
            get { return innerBindingElement; }
            set { innerBindingElement = value; }
        }

        //Main entry point into the encoder binding element. Called by WCF to get the factory that will create the
        //message encoder
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new CompressionMessageEncoderFactory(innerBindingElement.CreateMessageEncoderFactory());
        }

        public override MessageVersion MessageVersion
        {
            get { return innerBindingElement.MessageVersion; }
            set { innerBindingElement.MessageVersion = value; }
        }

        public override BindingElement Clone()
        {
            return new CompressionMessageEncodingBindingElement(this.innerBindingElement);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }
    }
}
