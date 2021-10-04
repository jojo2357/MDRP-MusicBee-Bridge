namespace MusicBeePlugin
{
	public class Settings 
	{
		public static Settings DEFAULT = new Settings();
		public bool KillOnClose { get; private set; }
		public bool AutoRun { get; private set; }
		public string MDRPLocation { get; private set; }

		private Settings()
		{
			MDRPLocation = "";
			AutoRun = false;
			KillOnClose = true;
		}

		public Settings(string loc, bool run, bool murder)
		{
			KillOnClose = murder;
			MDRPLocation = loc;
			AutoRun = run;
		}

		public string ToJson()
		{
			return "{\n" + MDRPLocation + "\n" +
			       AutoRun + "\n" +
			       KillOnClose + "\n" +
			       "}";
		}

		public static Settings FromJson(string json)
		{
			string[] lines = json.Split('\n');
			string loc = lines.Length > 0 ? lines[1].Trim() : "";
			bool run = lines.Length > 1 ? lines[2].Trim().ToLower() == "true" : true;
			bool murder = lines.Length > 2 ? lines[3].Trim().ToLower() == "true" : true;
			return new Settings(loc, run, murder);
		}
	}
}