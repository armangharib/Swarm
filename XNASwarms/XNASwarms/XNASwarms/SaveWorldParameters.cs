﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNASwarms
{
#if WINDOWS
    [Serializable]
#endif
    public class SaveWorldParameters
    {
        public int numberOfIndividualsMax;
        public int neighborhoodRadiusMax;
        public int normalSpeedMax;
        public int maxSpeedMax;
        public double c1Max;
        public double c2Max;
        public double c3Max;
        public double c4Max;
        public double c5Max;

        public SaveWorldParameters()
        {
        }
    }
}