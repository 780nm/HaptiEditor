using UnityEngine;
using System.IO.Ports;
using System;

namespace Haply.hAPI
{
    public class Board : MonoBehaviour
    {
        [HideInInspector] public string TargetPort;
        
        [SerializeField] private int baudRate;
        [SerializeField] private int serialTimeout;
        
        private bool hasBeenInitialized;

        public bool HasBeenInitialized => hasBeenInitialized;

        private SerialPort port;
        
        private void OnDestroy ()
        {
            if ( hasBeenInitialized || port is { IsOpen: true } )
            {
                ClosePort();
            }
        }

        public void DestroyBoard()
        {
            ClosePort();
            hasBeenInitialized = false;
        }
        
        /// <summary>
        /// Initializes the communication with a serial port, configuring its settings and attempting to open it.
        /// </summary>
        /// <remarks>
        /// This method checks if the board has already been initialized before proceeding.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during initialization.</exception>
        public void Initialize(string customPort = null)
        {
            if (hasBeenInitialized)
            {
                Debug.Log( "Board Already Initialized" );
                return;
            }
            try
            {
                if (customPort != null) TargetPort = customPort;
                port = new SerialPort(TargetPort, baudRate);
                port.ReadTimeout = serialTimeout;
                port.WriteTimeout = serialTimeout;
                port.DtrEnable = true;
                port.RtsEnable = true;

                port.Open();

                Debug.Log("Initialized Board: " + port.IsOpen);
                hasBeenInitialized = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Formats and transmits data over the serial port
        /// </summary>
        /// <param name="communicationType">type of communication taking place</param>
        /// <param name="deviceID">ID of device transmitting the information</param>
        /// <param name="bData">byte information to be transmitted</param>
        /// <param name="fData">float information to be transmitted</param>
        public void Transmit ( byte communicationType, byte deviceID, byte[] bData, float[] fData )
        {
            byte[] outData = new byte[2 + bData.Length + 4 * fData.Length];
            byte[] segments = new byte[4];

            outData[0] = communicationType;
            outData[1] = deviceID;

            Array.Copy( bData, 0, outData, 2, bData.Length );

            int j = 2 + bData.Length;
            foreach (float t in fData)
            {
                segments = FloatToBytes( t );
                Array.Copy( segments, 0, outData, j, 4 );
                j += 4;
            }

            port.Write( outData, 0, outData.Length );
        }
        
        private void ClosePort()
        {
            port.Close();

            hasBeenInitialized = false;
            Debug.Log( "Port closed" );
        }

        /// <summary>
        /// Receives data from the serial port and formats data to return a float data array
        /// </summary>
        /// <param name="deviceID">ID of the device receiving the information</param>
        /// <param name="expected">number for floating point numbers that are expected</param>
        /// <returns>formatted float data array from the received data</returns>
        public float[] Receive ( byte deviceID, int expected )
        {
            //Set_buffer(1 + 4 * expected);

            byte[] segments = new byte[4];

            byte[] inData = new byte[1 + 4 * expected];
            float[] data = new float[expected];

            port.Read( inData, 0, inData.Length );

            if ( inData[0] != deviceID )
            {
                Debug.LogError("Error, another device expects this data!");
            }

            int j = 1;

            for ( int i = 0; i < expected; i++ )
            {
                Array.Copy( inData, j, segments, 0, 4 );
                data[i] = BytesToFloat( segments );
                j = j + 4;
            }

            return data;
        }

        /// <summary>
        /// Checks if data is available from the board
        /// </summary>
        /// <returns>a boolean indicating if data is available from the serial port</returns>
        public bool DataAvailable() => port.BytesToRead > 0;

        /// <summary>
        /// Sends a reset command to perform a software reset of the Haply board
        /// </summary>
        public void ResetBoard () => Transmit( 0, 0, Array.Empty<byte>(), Array.Empty<float>() );

        /// <summary>
        /// Set serial buffer length for receiving incoming data
        /// </summary>
        /// <param name="length">number of bytes expected in read buffer</param>
        private void SetBuffer ( int length )
        {
            port.ReadBufferSize = length;
        }

        /// <summary>
        /// Translates a float point number to its raw binary format and stores it across four bytes
        /// </summary>
        /// <param name="val">floating point number</param>
        /// <returns>array of 4 bytes containing raw binary of floating point number</returns>
        private byte[] FloatToBytes ( float val )
        {
            return BitConverter.GetBytes(val);
        }

        /// <summary>
        /// Translates a binary of a float point to actual float point
        /// </summary>
        /// <param name="segment">array containing raw binary of floating point</param>
        /// <returns>translated floating point number</returns>
        private float BytesToFloat (byte[] segment) => BitConverter.ToSingle(segment, 0);

        /// <summary>
        /// Extracts a subarray of bytes from the given byte array, starting from the specified index 
        /// and extending for the specified length.
        /// </summary>
        /// <param name="data">The byte array from which to extract the subarray.</param>
        /// <param name="index">The zero-based starting index of the subarray in the source byte array.</param>
        /// <param name="length">The number of elements in the subarray.</param>
        /// <returns>A byte array containing the extracted subarray.</returns>
        public static byte[] SubArray (byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}