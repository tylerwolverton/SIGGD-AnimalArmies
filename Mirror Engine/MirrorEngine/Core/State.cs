using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class State
    {
        public int[] ints { get; set; }
        public float[] floats { get; set; }
        public bool[] bools { get; set; }
        public string[] strings { get; set; }
        public State baseState { get; set; }
        public State[] childStates { get; set; }

        //Only use if you are lazy
        public State()
        {
            ints = new int[10];
            floats = new float[10];
            bools = new bool[10];
            strings = new string[10];
            childStates = new State[10];
        }

        //Pass arguments in the order: (integers floats booleans strings child-states)
        public State(int intNum, int floatNum, int boolNum, int stringNum, int stateNum)
        {
            ints = new int[intNum];
            floats = new float[floatNum];
            bools = new bool[boolNum];
            strings = new string[stringNum];
            childStates = new State[stateNum];
        }
    }
}