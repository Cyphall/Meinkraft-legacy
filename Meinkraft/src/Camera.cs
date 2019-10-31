using GlmSharp;
using SFML.System;
using SFML.Window;

namespace Meinkraft
{
	public class Camera
	{
		private float _phi;
		private float _theta;

		private mat4 _projection;

		public vec3 position { get; private set; } = new vec3(8, 200, 8);
		private vec3 _orientation = vec3.Zero;
		private vec3 _sideOrientation = vec3.Zero;
		private vec3 _targetPoint = vec3.Zero;

		private Window _window;
		private Vector2i _winCenter;

		public Camera(Window window)
		{
			_window = window;
			_projection = mat4.Perspective(glm.Radians(82.0f), (float)window.Size.X / window.Size.Y, 0.01f, 1000.0f);
			_winCenter = new Vector2i((int)window.Size.X / 2, (int)window.Size.Y / 2);
			
			Mouse.SetPosition(_winCenter, _window);
			setRotation(0, 0);
		}
		
		private void setRotation(float vertical, float horizontal)
		{
			_phi = vertical;
			_theta = horizontal;
	
			_phi = (_phi > 89) ? 89 : (_phi < -89) ? -89 : _phi;
	
			float phiRadian = glm.Radians(_phi);
			float thetaRadian = glm.Radians(_theta);
			
			_orientation.x = glm.Cos(phiRadian) * glm.Sin(thetaRadian);
			_orientation.y = glm.Sin(phiRadian);
			_orientation.z = glm.Cos(phiRadian) * glm.Cos(thetaRadian);
	
			_sideOrientation = glm.Cross(new vec3(0, 1, 0), _orientation).Normalized;

			_targetPoint = position + _orientation;
		}

		private void rotate(float vertical, float horizontal)
		{
			setRotation(_phi + vertical, _theta + horizontal);
		}

		public void update()
		{
			float ratio = 1.0f;
			
			if(Keyboard.IsKeyPressed(Keyboard.Key.LControl))
			{
				ratio = 0.1f;
			}
			if(Keyboard.IsKeyPressed(Keyboard.Key.LShift))
			{
				ratio = 2f;
			}
	
			if(Keyboard.IsKeyPressed(Keyboard.Key.Z))
			{
				position += _orientation * 0.05f * ratio;
				_targetPoint = position + _orientation;
			}
			if(Keyboard.IsKeyPressed(Keyboard.Key.S))
			{
				position -= _orientation * 0.05f * ratio;
				_targetPoint = position + _orientation;
			}
			if(Keyboard.IsKeyPressed(Keyboard.Key.Q))
			{
				position += _sideOrientation * 0.05f * ratio;
				_targetPoint = position + _orientation;
			}
			if(Keyboard.IsKeyPressed(Keyboard.Key.D))
			{
				position -= _sideOrientation * 0.05f * ratio;
				_targetPoint = position + _orientation;
			}
	
			Vector2i mouseOffset = Mouse.GetPosition(_window) - _winCenter;
			
			if (mouseOffset.X + mouseOffset.Y == 0) return;
			
			rotate(-mouseOffset.Y / 10.0f, -mouseOffset.X / 10.0f);
			Mouse.SetPosition(_winCenter, _window);
		}

		public mat4 getViewProjection()
		{
			update();
			return _projection * mat4.LookAt(position, _targetPoint, new vec3(0, 1, 0));
		}

		public vec3 getPosition()
		{
			return position;
		}
	}
}