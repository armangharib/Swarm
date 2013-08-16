﻿using Microsoft.Xna.Framework;
using SwarmEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using XNASwarmsXAML.W8;
#endif

namespace XNASwarms.Emitters
{
    public interface IEmitterComponent
    {
        void Update(Vector2 position);
    }

    public class SwarmEmitterComponent : IEmitterComponent
    {
        public List<EmitterBase> Emitters
        {
            get;
            private set;
        }
        private PopulationSimulator populationSimulator;

        //TODO Should have an overload for loading emitters from saves
        public SwarmEmitterComponent(PopulationSimulator simulator)
        {
            populationSimulator = simulator;
            Emitters = new List<EmitterBase>();
            Emitters.Add(new BrushEmitter(new Vector2(0, 0)));
            Emitters.Add(new AudioEmitter(new Vector2(0, 0)));
        }

        //TODO this will be a collection of positons 
        public void Update(Vector2 position)
        {
            
            if (Emitters[0] is IGuideable)
            {
                ((IGuideable)Emitters[0]).UpdatePosition(position);
            }

            if (Emitters[0] is IMeteredAgents)
            {
                ((IMeteredAgents)Emitters[0]).CheckForSafeDistance(position);
            }

            if (Emitters[0].IsActive)
            {
                if (Emitters[0] is BrushEmitter)
                {
                    if (!StaticBrushParameters.IsUndo)
                    {
                        populationSimulator.EmitIndividual(Emitters[0].GetIndividual());
                    }
                    else
                    {
                        populationSimulator.UndoIndividual();
                    }
                }
                //else if(emitter is 
                else
                {
                    populationSimulator.EmitIndividual(Emitters[0].GetIndividual());
                }
            }   
        }
    }

public class SwarmAudioEmitterComponent : IEmitterComponent
    {

#if NETFX_CORE
        private IAudio audioScreen;
#endif
        public List<EmitterBase> Emitters
        {
            get;
            private set;
        }
        private PopulationSimulator populationSimulator;

#if NETFX_CORE
        public SwarmAudioEmitterComponent(PopulationSimulator simulator, IAudio audio)
        {
            audioScreen = audio;
            populationSimulator = simulator;
            Emitters = new List<EmitterBase>();
            Emitters.Add(new BrushEmitter(new Vector2(0, 0)));
            Emitters.Add(new AudioEmitter(new Vector2(0, 0)));
        }
#endif
        public void Update(Vector2 position)
        {

            if (Emitters[1] is IGuideable)
            {
                ((IGuideable)Emitters[1]).UpdatePosition(position);
            }


#if NETFX_CORE
            if (audioScreen != null && audioScreen.IsPlaying())
            {
                if (Emitters[1] is IAudioInfluenced)
                {
                    ((IAudioInfluenced)Emitters[1]).UpdateByAudio(audioScreen.GetFFTData());
                }
            }
#endif
            if (Emitters[1].IsActive)
            {
                populationSimulator.EmitIndividual(Emitters[1].GetIndividual());
            }
        }

    }
}
