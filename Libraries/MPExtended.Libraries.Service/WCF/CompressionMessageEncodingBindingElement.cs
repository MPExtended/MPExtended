//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
// Original code taken from Microsoft.ServiceModel.Samples
// Additional fixes from http://www.frenk.com/2009/12/gzip-compression-wcfsilverlight/

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
