using System;
using GLFW;
using GlmSharp;

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
		private readonly ivec2 _winCenter;

		protected Camera(Window window)
		{
			_window = window;
			
			_winCenter = new ivec2();
			Glfw.GetWindowSize(_window, out _winCenter.x, out _winCenter.y);
			
			Glfw.SetCursorPosition(_window, _winCenter.x, _winCenter.y);
			
			setRotation(0, 0);
		}

		private void setRotation(float vertical, float horizontal)
		{
			_phi = vertical;
			_theta = horizontal;

			_phi = (_phi > 89.9f) ? 89.9f : (_phi < -89f) ? -89.9f : _phi;

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

			if (Glfw.GetKey(_window, Keys.LeftControl) == InputState.Press)
			{
				ratio = 0.1f;
			}

			if (Glfw.GetKey(_window, Keys.LeftShift) == InputState.Press)
			{
				ratio = 2f;
			}

			if (Glfw.GetKey(_window, Keys.W) == InputState.Press)
			{
				position += orientation * 0.05f * ratio;
			}

			if (Glfw.GetKey(_window, Keys.S) == InputState.Press)
			{
				position -= orientation * 0.05f * ratio;
			}

			if (Glfw.GetKey(_window, Keys.A) == InputState.Press)
			{
				position += _sideOrientation * 0.05f * ratio;
			}

			if (Glfw.GetKey(_window, Keys.D) == InputState.Press)
			{
				position -= _sideOrientation * 0.05f * ratio;
			}
			
			dvec2 mouseOffset = new dvec2();
			Glfw.GetCursorPosition(_window, out mouseOffset.x, out mouseOffset.y);
			mouseOffset -= _winCenter;

			if (Math.Abs(mouseOffset.x + mouseOffset.y) < 0.01f) return;

			rotate((float)-mouseOffset.y / 10.0f, (float)-mouseOffset.x / 10.0f);
			Glfw.SetCursorPosition(_window, _winCenter.x, _winCenter.y);
		}

		public abstract void render();
	}
}