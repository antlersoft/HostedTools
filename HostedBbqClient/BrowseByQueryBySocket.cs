using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

using System.Xml.Serialization;

namespace com.antlersoft.BBQClient
{
    enum BBQSocketState
    {
        UNCONNECTED,
        CONNECTING,
        CONNECTED,
        CLOSING
    };
    public class BrowseByQueryBySocket : IBrowseByQuery, IDisposable
    {
        const int DEFAULT_PORT = 20217;
        const string DEFAULT_HOST = "localhost";

        XmlSerializer queryRequestSerializer;
        XmlSerializer queryResponseSerializer;
        BBQSocketState state;
        Socket socket;

        public BrowseByQueryBySocket()
        {
            queryRequestSerializer=new XmlSerializer( typeof(QueryRequest));
            queryResponseSerializer = new XmlSerializer(typeof(QueryResponse));
            state = BBQSocketState.UNCONNECTED;
        }

        public void Connect()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                state = BBQSocketState.CONNECTING;
                socket.Connect( new IPEndPoint( IPAddress.Loopback, DEFAULT_PORT));
                state = BBQSocketState.CONNECTED;
            }
            finally
            {
                if (state == BBQSocketState.CONNECTING)
                {
                    state = BBQSocketState.UNCONNECTED;
                    socket.Close();
                    socket = null;
                }
            }
        }

		public void Close()
		{
			if ( socket!=null && state!=BBQSocketState.UNCONNECTED)
                try
                {
                    socket.Close();
                }
                catch (Exception e)
                {
                }
			state=BBQSocketState.UNCONNECTED;
		}

		/**
		 * Interprets a sequence of four bytes as an integer in network byte order.
		 *
		 * @param array    Byte array that contains bytes to be interpreted as
		 * integer value
		 * @param offset   Position within array of the first byte to be interpreted
		 */
		public static int quadToInt( byte[] array, int offset)
		{
			return (array[offset]<<24)|(  array[offset+1]<<16)|
				(  array[offset+2]<<8)|( array[offset+3]);
		}

		/**
		 * Writes an integer to four bytes, in network byte order.
		 *
		 * @param value    Integer value to write into the byte array
		 * @param array    The byte array to receive the value
		 * @param offset   Position within the byte array that the first byte
		 * of the value should be written
		 */
		public static void intToQuad( int val, byte[] array, int offset)
		{
			array[offset+3]=(byte)(val&0xff);
			val>>=8;
			array[offset+2]=(byte)(val&0xff);
			val>>=8;
			array[offset+1]=(byte)(val&0xff);
			val>>=8;
			array[offset]=(byte)val;
		}

		public void writeFully( byte[] arr, int written, int len)
		{
			while ( written<len)
			{
				int wrote=socket.Send( arr, written, len-written, SocketFlags.None);
				if ( wrote<=0)
					throw new Exception( "Couldn't write");
				written+=wrote;
			}
		}

		public void readFully( byte[] arr)
		{
			int read=0;
			int len=arr.Length;
			while ( read<len)
			{
				int r=socket.Receive( arr, read, len-read, SocketFlags.None);
				if ( r<=0)
				throw new Exception( "Couldn't read");
				read+=r;
			}
		}

        #region IBrowseByQuery Members

        public QueryResponse PerformQuery(QueryRequest request)
        {
            try
            {
                if (state != BBQSocketState.CONNECTED)
                    Connect();
                StringWriter sw = new StringWriter();
                queryRequestSerializer.Serialize(sw, request);
                String requestString = sw.ToString();

                MemoryStream memory = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(memory, Encoding.UTF8);
                writer.Write(requestString.ToCharArray());
                writer.Close();
                byte[] requestBytes = memory.GetBuffer();
                byte[] len_buf = new byte[4];
                intToQuad(requestBytes.Length + 4, len_buf, 0);
                writeFully(len_buf, 0, 4);
                intToQuad(requestString.Length, len_buf, 0);
                writeFully(len_buf, 0, 4);
                writeFully(requestBytes, 0, requestBytes.Length);

                readFully(len_buf);
                int byte_len = quadToInt(len_buf, 0);
                byte[] response_bytes = new byte[byte_len];
                readFully(response_bytes);
                //int char_count=quadToInt( response_bytes, 0);

                memory = new MemoryStream(response_bytes, 4, byte_len - 4);
                StreamReader reader = new StreamReader(memory, Encoding.UTF8);
                //char[] chars=new char[char_count];
                //reader.Read( chars, 0, char_count);
                //Console.WriteLine( new String( chars));

                //memory=new MemoryStream( response_bytes, 4, byte_len-4);
                //reader=new StreamReader( memory, Encoding.UTF8);
                return (QueryResponse)queryResponseSerializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                Close();
                throw;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
			Close();
        }

        #endregion
    }
}
