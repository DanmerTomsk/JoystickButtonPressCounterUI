using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using JoystickButtonPressCounterUI.Dtos;

namespace JoystickButtonPressCounterUI.Observer
{
    internal sealed class JoystickObserver
    {
        private Dispatcher _uiDispatcher;

        public delegate void JoystickButtonPressEventHandler(uint joyId, byte buttonNumber);
        public delegate void JoystickButtonStateChangedEventHandler(uint joyId, ButtonsStateChange[] changes);

        public event JoystickButtonPressEventHandler? SomeButtonPress;
        public event JoystickButtonStateChangedEventHandler? ButtonsStateChanged;

        private Dictionary<uint, EventElement[]> _joyDelegatesDict = new Dictionary<uint, EventElement[]>();

        private Dictionary<uint, int> _joyButtonStateDict = new Dictionary<uint, int>();

        [DllImport("winmm.dll")]
        public static extern int joyGetPosEx(uint uJoyID, ref JoyInfoEx pji);

        public static JoystickObserver Observer { get; } = new JoystickObserver();

        public void ThreadFunc(object? param)
        {
            var parameters = param as ThreadParams;

            if (parameters == null)
            {
                return;
            }

            var joyId = parameters.JoyId;
            var delegates = parameters.Delegates;

            do
            {
                JoyInfoEx info = new JoyInfoEx();
                info.dwSize = Marshal.SizeOf(info);
                info.dwFlags = (int)JoyFlags.JOY_RETURNBUTTONS;
                int err = joyGetPosEx(joyId, ref info);
                if (err != 0) Console.Error.WriteLine((JoyErrors)err);
                else
                {
                    var changes = GetButtonStateChanges(joyId, info.dwButtons);

                    if (changes.Length != 0)
                    {
                        OnButtonsStateChanged(joyId, changes);
                    }

                    if (info.dwButtonNumber != 0)
                    {
                        var pressedButtonsNumbers = GetPressedButtonNumbers(info.dwButtons);
                        if (info.dwButtonNumber == 1)
                        {
                            OnOneButtonPressed(joyId, pressedButtonsNumbers.First());
                        }

                        foreach (var number in pressedButtonsNumbers)
                        {
                            var del = delegates[number]; 
                            
                            if (del is null)
                            {
                                continue;
                            }

                            _uiDispatcher.BeginInvoke(() => del.Dispatch(joyId, number));

                        }
                    }
                }

                Thread.Sleep(Configs.RequestIntervalInMilliseconds);
            } while (true);
        }

        public void AddToObserve(uint joyId, int number, JoystickButtonPressEventHandler handler)
        {
            var eventDelegate = GetDelegate(joyId, number);
            eventDelegate += handler;
        }

        public void RemoveFromObserve(uint joyId, int number, JoystickButtonPressEventHandler handler)
        {
            var eventDelegate = GetDelegate(joyId, number);
            eventDelegate -= handler;
        }

        public void Start(Dispatcher dispatcher)
        {
            _uiDispatcher = dispatcher;
            var joyIds = FindJoystick();

            foreach (var joyId in joyIds)
            {
                AddNewObserver(joyId);
            }
        }

        private ButtonsStateChange[] GetButtonStateChanges(uint joyId, int newState)
        {
            if (!_joyButtonStateDict.TryGetValue(joyId, out var oldState))
            {
                _joyButtonStateDict.Add(joyId, newState);
                return [];
            }

            var changedStates = newState ^ oldState;

            if (changedStates == 0)
            {
                return [];
            }

            _joyButtonStateDict[joyId] = newState;

            var result = new List<ButtonsStateChange>();
            for (byte i = 0; i < 32; i++)
            {
                var changedButtonNumber = changedStates & (1 << i);
                if (changedButtonNumber == 0)
                    continue;

                var isNowPressed = changedButtonNumber & newState;

                result.Add(new ButtonsStateChange(new JoyButtonInfo(joyId, i), isNowPressed == 0 ? false : true));
            }

            return result.ToArray();            
        }

        private byte[] GetPressedButtonNumbers(int dwButtons)
        {
            var result = new List<byte>();
            for (byte i = 0; i < 32; i++)
            {
                if ((dwButtons & (1 << i)) != 0)
                {
                    result.Add(i);
                }
            }

            return result.ToArray();
        }

        private EventElement GetDelegate(uint joyId, int number)
        {
            if (!_joyDelegatesDict.TryGetValue(joyId, out var delegates))
            {
                AddNewObserver(joyId);
                delegates = _joyDelegatesDict[joyId];
            }


            if (delegates[number] is null)
            {
                delegates[number] = new EventElement();
            }

            return delegates[number];
        }

        private void AddNewObserver(uint joyId)
        {
            var delegates = new EventElement[32];
            _joyDelegatesDict.Add(joyId, delegates);

            var thread = new Thread(ThreadFunc);
            thread.IsBackground = true;
            thread.Start(new ThreadParams(joyId, delegates));
        }

        private void OnOneButtonPressed(uint joyId, byte number)
        {
            if (SomeButtonPress != null)
            {
                _uiDispatcher.BeginInvoke(new Action(() => SomeButtonPress(joyId, number)));
            }
        }

        private void OnButtonsStateChanged(uint joyId, ButtonsStateChange[] changes)
        {
            if (ButtonsStateChanged != null)
            {
                ButtonsStateChanged(joyId, changes);
            }
        }

        private uint[] FindJoystick()
        {
            var result = new List<uint>();
            for (uint i = 0; i <= 15; i++)
            {
                JoyInfoEx info = new JoyInfoEx();
                info.dwSize = Marshal.SizeOf(info);
                info.dwFlags = (int)JoyFlags.JOY_RETURNBUTTONS;
                var err = (JoyErrors)joyGetPosEx(i, ref info);

                if (err == JoyErrors.JOYERR_NOERROR)
                {
                    Console.WriteLine($"JoyNum: {i}");
                    result.Add(i);
                }
                else
                {
                    Console.WriteLine($"{i}: {err}");
                }
            }

            return result.ToArray();
        }

        private sealed class ThreadParams
        {
            public uint JoyId { get; }

            public EventElement[] Delegates { get; }

            public ThreadParams(uint joyId, EventElement[] delegates)
            {
                JoyId = joyId;
                Delegates = delegates;
            }
        }

        private sealed class EventElement
        {
            protected event JoystickButtonPressEventHandler eventDelegate;

            public void Dispatch(uint joyId, byte buttonNumber)
            {
                if (eventDelegate != null)
                {                    
                    eventDelegate(joyId, buttonNumber);
                }
            }

            public static EventElement operator +(EventElement kElement, JoystickButtonPressEventHandler kDelegate)
            {
                kElement.eventDelegate += kDelegate;
                return kElement;
            }

            public static EventElement operator -(EventElement kElement, JoystickButtonPressEventHandler kDelegate)
            {
                kElement.eventDelegate -= kDelegate;
                return kElement;
            }
        }
    }
}
