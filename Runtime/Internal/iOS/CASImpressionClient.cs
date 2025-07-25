﻿//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_IOS || (CASDeveloper && UNITY_EDITOR)
using System;
namespace CAS.iOS
{
    public class CASImpressionClient : AdMetaData
    {
        // Required to free memory only for ( IntPtr )GCHandle.Alloc( instance );
        // Native refs are not needed for free.
        private IntPtr impressionRef;

        public CASImpressionClient(AdType type, IntPtr impressionRef) : base(type)
        {
            this.impressionRef = impressionRef;
        }

        public override AdNetwork network
        {
            get { return (AdNetwork)CASExterns.CASUGetImpressionNetwork(impressionRef); }
        }

        public override double revenue
        {
            get { return CASExterns.CASUGetImpressionRevenue(impressionRef); }
        }

        public override double cpm
        {
            get { return revenue * 1000.0; }
        }

        public override PriceAccuracy priceAccuracy
        {
            get { return (PriceAccuracy)CASExterns.CASUGetImpressionPrecission(impressionRef); }
        }

        public override string creativeIdentifier
        {
            get { return CASExterns.CASUGetImpressionCreativeId(impressionRef); }
        }

        public override string sourceUnitId
        {
            get { return CASExterns.CASUGetImpressionIdentifier(impressionRef); }
        }
        
        public override int impressionDepth
        {
            get { return CASExterns.CASUGetImpressionDepth(impressionRef); }
        }

        public override double lifetimeRevenue
        {
            get { return CASExterns.CASUGetImpressionLifetimeRevenue(impressionRef); }
        }
    }
}

#endif