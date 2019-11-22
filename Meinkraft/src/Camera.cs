using GlmSharp;
using SFML.System;
using SFML.Window;

namespace Meinkraft
{
	public abstract class Camera
	{
		private float _phi;
		private float _theta;

		public vec3 position { get; private set; } = new vec3(8, 200, 8);
		protected vec3 orientation = vec3.Zero;
		private vec3 _sideOrientation = vec3.Zero;

		private readonly Window _window;
		private readonly Vector2i _winCenter;

		protected Camera(Window window)
		{
			_window = window;

			_winCenter = new Vector2i((int) window.Size.X / 2, (int) window.Size.Y / 2);

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

			orientation.x = glm.Cos(phiRadian) * glm.Sin(thetaRadian);
			orientation.y = glm.Sin(phiRadian);
			orientation.z = glm.Cos(phiRadian) * glm.Cos(thetaRadian);

			_sideOrientation = glm.Cross(new vec3(0, 1, 0), orientation).Normalized;
		}

		private void rotate(float vertical, float horizontal)
		{
			setRotation(_phi + vertical, _theta + horizontal);
		}

		public void update()
		{
			float ratio = 1.0f;

			if (Keyboard.IsKeyPressed(Keyboard.Key.LControl))
			{
				ratio = 0.1f;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
			{
				ratio = 2f;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.Z))
			{
				position += orientation * 0.05f * ratio;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.S))
			{
				position -= orientation * 0.05f * ratio;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.Q))
			{
				position += _sideOrientation * 0.05f * ratio;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.D))
			{
				position -= _sideOrientation * 0.05f * ratio;
			}

			Vector2i mouseOffset = Mouse.GetPosition(_window) - _winCenter;

			if (mouseOffset.X + mouseOffset.Y == 0) return;

			rotate(-mouseOffset.Y / 10.0f, -mouseOffset.X / 10.0f);
			Mouse.SetPosition(_winCenter, _window);
		}

		public abstract void render();
	}
}