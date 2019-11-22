using System;
using System.Collections.Generic;

namespace Meinkraft
{
    public class CameraManager
    {
        private readonly Dictionary<string, Camera> _cameras = new Dictionary<string, Camera>();
        private string _currentCamera;

        public Camera current
        {
            get
            {
                if (!_cameras.ContainsKey(_currentCamera))
                    throw new KeyNotFoundException("This camera name isn't registered");
                return _cameras[_currentCamera];
            }
        }

        public void add(string name, Camera camera)
        {
            if (_cameras.ContainsKey(name))
                throw new ArgumentException("This camera name is already taken");
            _cameras.Add(name, camera);
            _currentCamera = name;
        }

        public void remove(string name)
        {
            if (!_cameras.ContainsKey(name))
                throw new KeyNotFoundException("This camera name isn't registered");
            _cameras.Remove(name);
        }

        public void setCurrent(string name)
        {
            if (!_cameras.ContainsKey(name))
                throw new KeyNotFoundException("This camera name isn't registered");
            _currentCamera = name;
        }
    }
}