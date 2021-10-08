namespace MusicBeePlugin
{
	public class Settings 
	{
		public static Settings DEFAULT = new Settings();
		public bool KillOnClose { get; private set; }
		public bool AutoRun { get; private set; }
		public string MDRPLocation { get; private set; }
		public string AssetPackName { get; private set; }

		private Settings()
		{
			MDRPLocation = "";
			AutoRun = false;
			KillOnClose = true;
			AssetPackName = "standard";
		}

		public Settings(string loc, bool run, bool murder, string packname)
		{
			KillOnClose = murder;
			MDRPLocation = loc;
			AutoRun = run;
			AssetPackName = packname;
		}

		public string ToJson()
		{
			return "{\n" + MDRPLocation + "\n" +
			       AutoRun + "\n" +
			       KillOnClose + "\n" +
			       AssetPackName + "\n" +
			       "}";
		}

		public static Settings FromJson(string json)
		{
			string[] lines = json.Split('\n');
			string loc = lines.Length > 2 ? lines[1].Trim() : "";
			bool run = lines.Length > 3 ? lines[2].Trim().ToLower() == "true" : true;
			bool murder = lines.Length > 4 ? lines[3].Trim().ToLower() == "true" : true;
			string pack = lines.Length > 5 ? lines[4].Trim() : "standard";
			return new Settings(loc, run, murder, pack);
		}
	}

	public class SkinSelectorItem
	{
		public int ID { get; set; }
		public string Text { get; set; }

		public override string ToString()
		{
			return Text;
		}
	}
}