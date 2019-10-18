﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Foster.Framework
{
    public class Controller
    {
        public const int MaxButtons = 64;
        public const int MaxAxis = 64;

        public string Name { get; private set; } = "Unknown";
        public bool Connected { get; private set; } = false;
        public bool IsGamepad { get; private set; } = false;
        public int Buttons { get; private set; } = 0;
        public int Axes { get; private set; } = 0;

        internal readonly bool[] pressed = new bool[MaxButtons];
        internal readonly bool[] down = new bool[MaxButtons];
        internal readonly bool[] released = new bool[MaxButtons];
        internal readonly ulong[] timestamp = new ulong[MaxButtons];
        internal readonly float[] axis = new float[MaxAxis];
        internal readonly ulong[] axisTimestamp = new ulong[MaxAxis];

        internal void Connect(string name, uint buttonCount, uint axisCount, bool isGamepad)
        {
            Name = name;
            Buttons = (int)Math.Min(buttonCount, MaxButtons);
            Axes = (int)Math.Min(axisCount, MaxAxis);
            IsGamepad = isGamepad;
        }

        internal void Disconnect()
        {
            Name = "Unknown";
            Connected = false;
            IsGamepad = false;
            Buttons = 0;
            Axes = 0;

            Array.Fill(pressed, false);
            Array.Fill(down, false);
            Array.Fill(released, false);
            Array.Fill(timestamp, 0UL);
            Array.Fill(axis, 0);
            Array.Fill(axisTimestamp, 0UL);
        }

        internal void Step()
        {
            Array.Fill(pressed, false);
            Array.Fill(released, false);
        }

        internal void Copy(Controller other)
        {
            Name = other.Name;
            Connected = other.Connected;
            IsGamepad = other.IsGamepad;
            Buttons = other.Buttons;
            Axes = other.Axes;

            Array.Copy(other.pressed, 0, pressed, 0, Buttons);
            Array.Copy(other.down, 0, down, 0, Buttons);
            Array.Copy(other.released, 0, released, 0, Buttons);
            Array.Copy(other.timestamp, 0, timestamp, 0, Buttons);
            Array.Copy(other.axis, 0, axis, 0, Axes);
            Array.Copy(other.axisTimestamp, 0, axisTimestamp, 0, Axes);
        }

        public bool Pressed(int buttonIndex) => buttonIndex >= 0 && buttonIndex < Buttons && pressed[buttonIndex];
        public bool Pressed(Buttons button) => Pressed((int)button);

        public ulong Timestamp(int buttonIndex) => buttonIndex >= 0 && buttonIndex < Buttons ? timestamp[buttonIndex] : 0;
        public ulong Timestamp(Buttons button) => Timestamp((int)button);

        public bool Down(int buttonIndex) => buttonIndex >= 0 && buttonIndex < Buttons && down[buttonIndex];
        public bool Down(Buttons button) => Down((int)button);

        public bool Released(int buttonIndex) => buttonIndex >= 0 && buttonIndex < Buttons && released[buttonIndex];
        public bool Released(Buttons button) => Released((int)button);

        public float Axis(int axisIndex) => (axisIndex >= 0 && axisIndex < Axes) ? axis[axisIndex] : 0f;
        public float Axis(Axes axis) => Axis((int)axis);

        public Vector2 Axis(int axisX, int axisY) => new Vector2(Axis(axisX), Axis(axisY));
        public Vector2 Axis(Axes axisX, Axes axisY) => new Vector2(Axis(axisX), Axis(axisY));

        public Vector2 LeftStick => Axis(Framework.Axes.LeftX, Framework.Axes.LeftY);
        public Vector2 RightStick => Axis(Framework.Axes.RightX, Framework.Axes.RightY);

    }
}
