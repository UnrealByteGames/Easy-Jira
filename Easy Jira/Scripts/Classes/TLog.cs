using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnrealByte.EasyJira {

	public class UBLog {

		public string logFilePath = "";
		public bool debugLog;
		public bool includeWarnings;

		public UBLog() {

		}

		public UBLog(bool initialize) {

		}

		/// <summary>
		/// Initializes the log.
		/// </summary>
		/// <returns>The log file path.</returns>
		/// <param name="inCustomPath">If set to <c>true</c> save the log in custom path.</param>
		/// <param name="customFilePath">If inCustomPath is set to true, the uses this custom file path.</param>
		/// <param name="initMessage">Init message. Is an info header for log file.</param>
		/// <param name="debugLog">If set to <c>true</c> then include the debug log (Debug.Log()).</param>
		/// <param name="includeWarnings">If debugLog set to <c>true</c> the use this <c>false</c> to ignore warnings from Debug.Log().</param>
		public string initializeLog(bool inCustomPath, string customFilePath, string initMessage, bool debugLog, bool includeWarnings){
			logFilePath = "";
			this.debugLog = debugLog;
			this.includeWarnings = includeWarnings;
			if (!inCustomPath) {
				logFilePath = Application.persistentDataPath + "/TLog.txt";
			} else {
				logFilePath = customFilePath + "/TLog.txt";
			}

			if (debugLog) {
				Application.logMessageReceivedThreaded += HandleLog;
			}

			StreamWriter fileWriter = File.CreateText(logFilePath);
			fileWriter.WriteLine("============= Begin Log ===============");
			fileWriter.WriteLine("");
			fileWriter.WriteLine("["+System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")+"]");
			if (debugLog) {
				fileWriter.WriteLine ("Debug Log is active!");
			}
			fileWriter.WriteLine("================= Init Message ===================");
			fileWriter.WriteLine (initMessage);
			fileWriter.WriteLine("==================================================");
			fileWriter.WriteLine("");
			fileWriter.Close();

			return logFilePath;
		}

		/// <summary>
		/// Log the specified msg.
		/// </summary>
		/// <param name="msg">Message.</param>
		public void log (string msg) {
			if (logFilePath.Length == 0) {
				initializeLog (false, "", "Auto generated log. Probably you called the method UBLog.log() without previous initialization.", true, false);
			}
			StreamWriter fileWriter = new StreamWriter (logFilePath, true);
			fileWriter.WriteLine("["+System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")+"] - " + msg);
			fileWriter.Close ();
		}

		/// <summary>
		/// Handles the Debug.log() callback.
		/// </summary>
		/// <param name="condition">Condition.</param>
		/// <param name="stackTrace">Stack trace.</param>
		/// <param name="type">Type.</param>
		private void HandleLog(string condition, string stackTrace, LogType type) {

			if (includeWarnings) {
				string logEntry = string.Format("\n {0} {1} \n {2}\n {3}" , System.DateTime.Now, type, condition, stackTrace);
				log(logEntry);
			} else {
				if(!type.ToString().Equals (LogType.Warning.ToString())) {
					string logEntry = string.Format("\n {0} {1} \n {2}\n {3}" , System.DateTime.Now, type, condition, stackTrace);
					log(logEntry);
				}
		}
		}

	}
}