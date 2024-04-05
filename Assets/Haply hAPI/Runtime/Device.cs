using System;
using UnityEngine;

namespace Haply.hAPI
{
    public class Device : MonoBehaviour
    {
	    [SerializeField] private DeviceConfig configData;
        [SerializeField] private byte deviceID = 9;
        [SerializeField] private Mechanism mechanism;
        [SerializeField] private Board boardLink;

        private byte commmunicationType;
        private int actuatorsActive;
        private int encodersActive;
        private int sensorsActive;
        private int pwmsActive;

        private Actuator[] motors = Array.Empty<Actuator>();
        private Sensor[] encoders = Array.Empty<Sensor>();
        private Sensor[] sensors = Array.Empty<Sensor>();
        private Pwm[] pwms = Array.Empty<Pwm>();

        private readonly byte[] actuatorPositions = { 0, 0, 0, 0 };
        private readonly byte[] encoderPositions = { 0, 0, 0, 0 };

        private void Awake()
        {
	        if(mechanism == null) mechanism = FindObjectOfType<Mechanism>();
	        if(boardLink == null) boardLink = FindObjectOfType<Board>();
        }

        public bool CheckButtonFlipped() => configData.FlippedStylusButton;

        /// <summary>
		/// add new actuator to platform
		/// </summary>
		/// <param name="actuator">index of actuator (an index of 1-4)</param>
		/// <param name="rotation">positive direction of actuator rotation</param>
		/// <param name="port">specified motor port to be used (motor ports 1-4 on the Haply board)</param>
		private void AddActuator ( int actuator, int rotation, int port )
		{
			bool error = false;

			if ( port is < 1 or > 4 )
			{
				Debug.LogError( " encoder port index out of bounds" );
				error = true;
			}

			if ( actuator is < 1 or > 4 )
			{
				Debug.LogError( " encoder index out of bound!" );
				error = true;
			}

			int j = 0;
			for ( int i = 0; i < actuatorsActive; i++ )
			{
				if ( motors[i].ActuatorIndex < actuator )
				{
					j++;
				}

				if (motors[i].ActuatorIndex != actuator) continue;
				Debug.LogError( " actuator " + actuator + " has already been set" );
				error = true;
			}
			if (error) return;
			
			Actuator[] temp = new Actuator[actuatorsActive + 1];

			Array.Copy( motors, 0, temp, 0, motors.Length );

			if ( j < actuatorsActive )
			{
				Array.Copy( motors, j, temp, j + 1, motors.Length - j );
			}

			temp[j] = new Actuator( actuator, rotation, port );
			ActuatorAssignment( actuator, port );

			motors = temp;
			actuatorsActive++;
		}
		
		/// <summary>
		/// Add a new encoder to the platform
		/// </summary>
		/// <param name="encoder">index of actuator (an index of 1-4)</param>
		/// <param name="rotation">positive direction of rotation detection</param>
		/// <param name="offset">encoder offset in degrees</param>
		/// <param name="resolution">encoder resolution</param>
		/// <param name="port">specified motor port to be used</param>
		private void AddEncoder ( int encoder, int rotation, float offset, float resolution, int port )
		{
			bool error = false;

			if ( port is < 1 or > 4 )
			{
				Debug.LogError( " encoder port index out of bounds" );
				error = true;
			}

			if ( encoder is < 1 or > 4 )
			{
				Debug.LogError( " encoder index out of bound!" );
				error = true;
			}

			// determine index for copying
			int j = 0;
			for ( int i = 0; i < encodersActive; i++ )
			{
				if ( encoders[i].Encoder < encoder )
				{
					j++;
				}

				if (encoders[i].Encoder != encoder) continue;
				Debug.LogError( " encoder " + encoder + " has already been set" );
				error = true;
			}
			if (error) return;
			
			Sensor[] temp = new Sensor[encodersActive + 1];

			Array.Copy( encoders, 0, temp, 0, encoders.Length );

			if ( j < encodersActive )
			{
				Array.Copy( encoders, j, temp, j + 1, encoders.Length - j );
			}

			temp[j] = new Sensor( encoder, rotation, offset, resolution, port );
			EncoderAssignment( encoder, port );

			encoders = temp;
			encodersActive++;
		}

		/// <summary>
		/// Add an analog sensor to platform
		/// </summary>
		/// <param name="pin">the analog pin on haply board to be used for sensor input (Ex: A0)</param>
		private void AddAnalogSensor ( String pin )
		{
			// set sensor to be size zero
			bool error = false;

			char port = pin[0];
			String number = pin.Substring( 1 );

			int value = int.Parse( number );
			value += 54;

			for ( int i = 0; i < sensorsActive; i++ )
			{
				if (value != sensors[i].Port) continue;
				Debug.LogError( " Analog pin: A" + (value - 54) + " has already been set" );
				error = true;
			}

			if ( port != 'A' || value < 54 || value > 65 )
			{
				Debug.LogError( " outside analog pin range" );
				error = true;
			}

			if (error) return;
			Sensor[] temp = new Sensor[sensors.Length + 1]; 
			Array.Copy( sensors, temp, sensors.Length );
			temp[sensorsActive] = new Sensor
			{
				Port = (value)
			};
			sensors = temp;
			sensorsActive++;
		}

		/// <summary>
		/// Add a PWM output pin to the platform
		/// </summary>
		/// <param name="pin">the pin on the haply board to use as the PWM output pin</param>
		public void AddPwmPin ( int pin )
		{
			bool error = false;
			
			for ( int i = 0; i < pwmsActive; i++ )
			{
				if (pin != pwms[i].Pin) continue;
				Debug.LogError( " pwm pin: " + pin + " has already been set" );
				error = true;
			}

			switch (pin)
			{
				case < 0:
				case > 13:
					Debug.LogError( " outside pwn pin range" );
					error = true;
					break;
				case 0:
				case 1:
					Debug.LogWarning( "0 and 1 are not pwm pins on Haply M3 or Haply original" );
					break;
			}

			if (error) return;
			Pwm[] temp = new Pwm[pwms.Length + 1];
			Array.Copy( pwms, temp, pwms.Length );
			temp[pwmsActive] = new Pwm
			{
				Pin = (pin)
			};
			pwms = temp;
			pwmsActive++;
		}

		/// <summary>
		/// Set the device mechanism that is to be used
		/// </summary>
		/// <param name="mech">new Mechanisms for use</param>
		public void SetMechanism ( Mechanism mech )
		{
			mechanism = mech;
		}
		
		/// <summary>
		/// Loads device specific data from a scriptable object
		/// </summary>
		public void LoadConfig(DeviceConfig customConfig = null)
		{
			DeviceConfig targetConfig = customConfig ?? configData;
			mechanism.SetBoardType(targetConfig.BoardType);
			ActuatorRotations actuator = targetConfig.ActuatorRotations;
			EncoderRotations encoder = targetConfig.EncoderRotations;
			Offset offset = targetConfig.Offset;
			int resolution = targetConfig.Resolution;
            
			AddActuator(1, (int)actuator.Rotation1, 2);
			AddActuator(2, (int)actuator.Rotation2, 1);
			AddEncoder(1, (int)encoder.Rotation1, offset.Left, resolution, 2);
			AddEncoder(2, (int)encoder.Rotation2, offset.Right, resolution, 1);
			AddAnalogSensor("A2");
		}

		/// <summary>
		/// Gathers all encoder, sensor, pwm setup information
		/// of all encoders, sensors, and pwm pins that are
		/// initialized and sequentially formats the data based
		/// on specified sensor index positions to send over
		/// serial port interface for hardware device initialization 
		/// </summary>
		public void DeviceSetParameters ()
		{
			commmunicationType = 1;

			int control;

			float[] encoderParameters;

			byte[] encoderParams;
			byte[] motorParams;
			byte[] sensorParams;
			byte[] pwmParams;

			if (encodersActive > 0)
			{
				encoderParams = new byte[encodersActive + 1];
				control = 0;

				for ( int i = 0; i < encoders.Length; i++ )
				{
					if ( encoders[i].Encoder != (i + 1) )
					{
						Debug.LogWarning( "improper encoder indexing" );
						encoders[i].Encoder = ( i + 1 );
						encoderPositions[encoders[i].Port - 1] = (byte) encoders[i].Encoder;
					}
				}

				foreach (byte t in encoderPositions)
				{
					control >>= 1;

					if ( t > 0 )
					{
						control |= 0x0008;
					}
				}

				encoderParams[0] = (byte) control;

				encoderParameters = new float[2 * encodersActive];

				int j = 0;
				foreach (byte t in encoderPositions)
				{
					if (t <= 0) continue;
					encoderParameters[2 * j] = encoders[t - 1].EncoderOffset;
					encoderParameters[2 * j + 1] = encoders[t - 1].EncoderResolution;
					j++;
					encoderParams[j] = (byte) encoders[t - 1].Direction;
				}
			}
			else
			{
				encoderParams = new byte[1];
				encoderParams[0] = 0;
				encoderParameters = Array.Empty<float>();
			}


			if ( actuatorsActive > 0 )
			{
				motorParams = new byte[actuatorsActive + 1];
				control = 0;

				for ( int i = 0; i < motors.Length; i++ )
				{
					if (motors[i].ActuatorIndex == (i + 1)) continue;
					Debug.LogWarning( "improper actuator indexing" );
					motors[i].ActuatorIndex = ( i + 1 );
					actuatorPositions[motors[i].Port - 1] = (byte) motors[i].ActuatorIndex;
				}

				foreach (byte t in actuatorPositions)
				{
					control >>= 1;

					if ( t > 0 )
					{
						control |= 0x0008;
					}
				}

				motorParams[0] = (byte) control;

				int j = 1;
				foreach (byte actuatorPosition in actuatorPositions)
				{
					if (actuatorPosition <= 0) continue;
					motorParams[j] = (byte) motors[actuatorPosition - 1].Direction;
					j++;
				}
			}
			else
			{
				motorParams = new byte[1];
				motorParams[0] = 0;
			}


			if ( sensorsActive > 0 )
			{
				sensorParams = new byte[sensorsActive + 1];
				sensorParams[0] = (byte) sensorsActive;

				for ( int i = 0; i < sensorsActive; i++ )
				{
					sensorParams[i + 1] = (byte) sensors[i].Port;
				}

				Array.Sort( sensorParams );

				for ( int i = 0; i < sensorsActive; i++ )
				{
					sensors[i].Port = ( sensorParams[i + 1] );
				}

			}
			else
			{
				sensorParams = new byte[1];
				sensorParams[0] = 0;
			}


			if ( pwmsActive > 0 )
			{
				byte[] temp = new byte[pwmsActive];

				pwmParams = new byte[pwmsActive + 1];
				pwmParams[0] = (byte) pwmsActive;


				for ( int i = 0; i < pwmsActive; i++ )
				{
					temp[i] = (byte) pwms[i].Pin;
				}

				Array.Sort( temp );

				for ( int i = 0; i < pwmsActive; i++ )
				{
					pwms[i].Pin = ( temp[i] );
					pwmParams[i + 1] = (byte) pwms[i].Pin;
				}

			}
			else
			{
				pwmParams = new byte[1];
				pwmParams[0] = 0;
			}


			byte[] encMtrSenPwm = new byte[motorParams.Length + encoderParams.Length + sensorParams.Length + pwmParams.Length];
			Array.Copy( motorParams, 0, encMtrSenPwm, 0, motorParams.Length );
			Array.Copy( encoderParams, 0, encMtrSenPwm, motorParams.Length, encoderParams.Length );
			Array.Copy( sensorParams, 0, encMtrSenPwm, motorParams.Length + encoderParams.Length, sensorParams.Length );
			Array.Copy( pwmParams, 0, encMtrSenPwm, motorParams.Length + encoderParams.Length + sensorParams.Length, pwmParams.Length );

			boardLink.Transmit( commmunicationType, deviceID, encMtrSenPwm, encoderParameters );
		}
		
		/// <summary>
		/// Assigns an actuator to a specified port.
		/// </summary>
		/// <param name="actuator">The identifier of the actuator to assign.</param>
		/// <param name="port">The port number to which the actuator is to be assigned.</param>
		private void ActuatorAssignment ( int actuator, int port )
		{
			if ( actuatorPositions[port - 1] > 0 )
			{
				Debug.LogWarning( "double check actuator port usage" );
			}

			actuatorPositions[port - 1] = (byte) actuator;
		}

		/// <summary>
		/// Assigns an encoder to a specified port.
		/// </summary>
		/// <param name="encoder">The identifier of the encoder to assign.</param>
		/// <param name="port">The port number to which the encoder is to be assigned.</param>
		private void EncoderAssignment ( int encoder, int port )
		{
			if ( encoderPositions[port - 1] > 0 )
			{
				Debug.LogWarning( "double check encoder port usage" );
			}

			encoderPositions[port - 1] = (byte) encoder;
		}

		/// <summary>
		/// Receives angle position and sensor information from the serial port interface
		/// and updates each indexed encoder sensor to their respective received angle
		/// and any analog sensor that may be setup
		/// </summary>
		public void DeviceReadData ()
		{
			commmunicationType = 2;
			int dataCount = 0;

			float[] deviceData = boardLink.Receive(deviceID, sensorsActive + encodersActive);

			for ( int i = 0; i < sensorsActive; i++ )
			{
				sensors[i].Value = ( deviceData[dataCount] );
				dataCount++;
			}

			foreach (byte position in encoderPositions)
			{
				if (position <= 0) continue;
				encoders[position - 1].Value = ( deviceData[dataCount] );
				dataCount++;
			}
		}

		/// <summary>
		/// Requests data from the hardware based on the initialized setup.
		/// Function also sends a torque output command of zero torque for each actuator in use
		/// </summary>
		public void DeviceReadRequest ()
		{
			commmunicationType = 2;
			byte[] pulses = new byte[pwmsActive];
			float[] encoderRequest = new float[actuatorsActive];

			for ( int i = 0; i < pwms.Length; i++ )
			{
				pulses[i] = (byte) pwms[i].Value;
			}

			int j = 0;
			foreach (byte actuatorPosition in actuatorPositions)
			{
				if (actuatorPosition <= 0) continue;
				encoderRequest[j] = 0;
				j++;
			}

			boardLink.Transmit( commmunicationType, deviceID, pulses, encoderRequest );
		}

		/// <summary>
		/// Transmits specific torques that has been calculated and stored
		/// for each actuator over the serial port interface.
		/// Also transmits specified pwm outputs on pwm pins
		/// </summary>
		public void DeviceWriteTorques ()
		{
			commmunicationType = 2;
			byte[] pulses = new byte[pwmsActive];
			float[] deviceTorques = new float[actuatorsActive];

			for ( int i = 0; i < pwms.Length; i++ )
			{
				pulses[i] = (byte) pwms[i].Value;
			}

			int j = 0;
			foreach (byte actuatorPosition in actuatorPositions)
			{
				if (actuatorPosition <= 0) continue;
				deviceTorques[j] = motors[actuatorPosition - 1].Torque;
				j++;
			}

			boardLink.Transmit( commmunicationType, deviceID, pulses, deviceTorques );
		}
		
		/// <summary>
		/// Set pulse of specified PWM pin
		/// </summary>
		/// <param name="pin">target pin</param>
		/// <param name="pulse">pulse value</param>
		public void SetPwmPulse ( int pin, float pulse )
		{
			foreach (Pwm pwm in pwms)
			{
				if ( pwm.Pin == pin )
				{
					pwm.SetPulse( pulse );
				}
			}
		}
		
		/// <summary>
		/// Gets percent PWM pulse value of specified pin
		/// </summary>
		/// <param name="pin">target pin</param>
		/// <returns>pulse value</returns>
		public float GetPwmPulse ( int pin )
		{
			float pulse = 0;

			foreach (Pwm pwm in pwms)
			{
				if ( pwm.Pin == pin )
				{
					pulse = pwm.GetPulse();
				}
			}

			return pulse;
		}

		/// <summary>
		/// Gathers current state of angles information from encoder objects
		/// </summary>
		/// <param name="buffer">reference buffer for most recent angles information from encoder objects</param>
		public void GetDeviceAngles ( ref float[] buffer )
		{
			if ( buffer == null || buffer.Length != encodersActive )
			{
				buffer = new float[encodersActive];
			}

			for ( int i = 0; i < encodersActive; i++ )
			{
				buffer[i] = encoders[i].Value;
			}
		}

		/// <summary>
		/// Gathers current state of angles information from encoder objects
		/// </summary>
		/// <param name="buffer">buffer for most recent angles information from encoder objects</param>
		public void GetDeviceAnglesUnchecked (ref float[] buffer )
		{
			for ( int i = 0; i < encodersActive; i++ )
			{
				buffer[i] = encoders[i].Value;
			}
		}
		
		/// <summary>
		/// Gathers current state of the angular velocity information from encoder objects
		/// </summary>
		/// <param name="buffer">reference buffer for most recent angles information from encoder objects</param>
		public void GetDeviceAngularVelocities ( ref float[] buffer )
		{
			if ( buffer == null || buffer.Length != encodersActive )
			{
				buffer = new float[encodersActive];
			}

			for ( int i = 0; i < encodersActive; i++ )
			{
				buffer[i] = encoders[i].Velocity;
			}
		}
		
		/// <summary>
		/// Gathers current data from sensor objects
		/// </summary>
		/// <param name="buffer">reference buffer for most recent analog sensor information from sensor objects</param>
		public void GetSensorData ( ref float[] buffer )
		{
			if ( buffer == null || buffer.Length != sensorsActive )
            {
				buffer = new float[sensorsActive];
			}

			for ( int i = 0; i < sensorsActive; i++ )
			{
				buffer[i] = sensors[i].Value;
			}
		}
		
		/// <summary>
		/// Performs physics calculations
		/// and retrieves the current position data of a device based on the provided angles.
		/// </summary>
		/// <param name="angles">angles for physics calculations</param>
		/// <param name="buffer">buffer to store end effector coordinate position</param>
		public void GetDevicePosition ( float[] angles, float[] buffer )
		{
			mechanism.ForwardKinematics( angles );
			mechanism.GetCoordinate( buffer );
		}

		/// <summary>
		/// Gathers current state of angles information from encoder objects
		/// </summary>
		/// <param name="angularVelocities">angular velocities for velocity calculations</param>
		/// <param name="buffer">Buffer to store calculated velocities of the end effector</param>
		public void GetDeviceVelocities ( float[] angularVelocities, float[] buffer )
		{
			mechanism.VelocityCalculation( angularVelocities );
			mechanism.GetVelocity( buffer );
		}

		/// <summary>
		/// Calculates and applies torques to the device's actuators based on the provided forces.
		/// </summary>
		/// <param name="forces">forces to calculate torques for each actuator</param>
		/// <param name="torques">buffer to store the calculated torques for each actuator</param>
		public void SetDeviceTorques ( float[] forces, float[] torques )
		{
			mechanism.TorqueCalculation( forces );
			mechanism.GetTorque( torques );

			for ( int i = 0; i < actuatorsActive; i++ )
			{
				motors[i].Torque = torques[i];
			}
		}
	}
}