using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HanlangGame.Engine
{
    public class Input
    {
        private readonly HashSet<Keys> _currentKeys = new();
        private readonly HashSet<Keys> _previousKeys = new();

        // 대시(달리기) 감지를 위한 더블탭 타이머
        private Keys _lastTapKey = Keys.None;
        private DateTime _lastTapTime = DateTime.MinValue;
        public bool IsDashing { get; private set; }

        public void Update()
        {
            _previousKeys.Clear();
            foreach (var key in _currentKeys)
            {
                _previousKeys.Add(key);
            }
        }

        public void OnKeyDown(Keys key)
        {
            if (!_currentKeys.Contains(key))
            {
                // 좌/우 더블탭 감지 (0.28초 이내 재입력)
                if (key == Keys.Left || key == Keys.Right)
                {
                    var now = DateTime.UtcNow;
                    if (_lastTapKey == key && (now - _lastTapTime).TotalMilliseconds < 280)
                    {
                        IsDashing = true;
                    }
                    _lastTapKey = key;
                    _lastTapTime = now;
                }
            }
            _currentKeys.Add(key);
        }

        public void OnKeyUp(Keys key)
        {
            _currentKeys.Remove(key);
            if (key == Keys.Left || key == Keys.Right)
            {
                if (!IsHeld(Keys.Left) && !IsHeld(Keys.Right))
                {
                    IsDashing = false;
                }
            }
        }

        public bool IsHeld(Keys key) => _currentKeys.Contains(key);
        public bool IsJustPressed(Keys key) => _currentKeys.Contains(key) && !_previousKeys.Contains(key);
        public bool IsJustReleased(Keys key) => !_currentKeys.Contains(key) && _previousKeys.Contains(key);

        public void Reset()
        {
            _currentKeys.Clear();
            _previousKeys.Clear();
            IsDashing = false;
        }
    }
}
