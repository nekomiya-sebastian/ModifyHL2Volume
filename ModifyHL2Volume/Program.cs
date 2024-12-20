Main.Go();

static class NekoUtils
{
	public static string ModifyLine( string line,float modifyAmount,ref bool hadError )
	{
		var vals = new List<string>();
		var inds = new List<int>();

		for( int i = 0; i < line.Length - 1; ++i )
		{
			var c = line[i];

			if( c == '"' )
			{
				string val = "";
				for( int j = i + 1; j < line.Length; ++j )
				{
					var c2 = line[j];
					if( !( Char.IsNumber( c2 ) || c2 == '.' || c2 == '"' || c2 == ',' ) ) break;
					if( c2 == '"' )
					{
						vals.Add( val );
						inds.Add( j - val.Length );
						j = line.Length + 1;
						i = line.Length + 1;
					}
					else if( c2 == ',' )
					{
						vals.Add( val );
						inds.Add( j - val.Length );
						val = "";
					}
					else val += c2;
				}
			}
		}
		
		string newLine = "";
		for( int j = 0; j < line.Length; ++j ) newLine += line[j];

		// go backwards so we don't invalidate inds
		for( int i = vals.Count - 1; i >= 0; --i )
		{
			var curVal = vals[i];
			var curInd = inds[i];
			try
			{
				var newVal = float.Parse( curVal ) * modifyAmount;
				newLine = newLine.Remove( curInd,curVal.Length );
				newLine = newLine.Insert( curInd,newVal.ToString( "0.##" ) );
			}
			catch( Exception )
			{
				Console.WriteLine( "Invalid line input, unable to modify volume! Attempted parse value: \"" +
					curVal + "\" Line: " + line );
				hadError = true;
			}
		}

		return( newLine );
	}
}

class Main
{
	public static void Go()
	{
		bool readInput = false;
		float volModifyPercent = 0.0f;
		Console.Write( "Input volume modify % (0-1):" );
		while( !readInput )
		{
			var result = Console.ReadLine();
			if( result == null ) continue;
			try
			{
				volModifyPercent = float.Parse( result );
				if( volModifyPercent < 0 )
				{
					Console.Write( "You can't make a negative sound, baka! " );
					throw( new Exception() );
				}
			}
			catch( Exception )
			{
				Console.Write( "Invalid input, please retry: " );
				continue;
			}
			readInput = true;
		}
		
		var filePath = "game_sounds_weapons.txt";
		var scriptsPath = "scripts/";
		bool hadError = false;
		if( !File.Exists( scriptsPath + filePath ) )
		{
			Console.WriteLine( "Couldn't find file!" );
			hadError = true;
		}
		else
		{
			var reader = new StreamReader( scriptsPath + filePath );
			var lines = new List<string>();
			while( !reader.EndOfStream )
			{
				var line = reader.ReadLine();
				if( line != null ) lines.Add( line );
			}
			
			var modifiedLines = new List<string>();
			
			foreach( var line in lines )
			{
				if( line.Contains( "\"volume\"") )
				{
					modifiedLines.Add( NekoUtils.ModifyLine( line,volModifyPercent,ref hadError ) );
				}
				else modifiedLines.Add( line );
			}

			var modsPath = "custom/my_mods/scripts/";
			var newFilePath = modsPath + filePath;
			if( !Directory.Exists( modsPath ) ) Directory.CreateDirectory( modsPath );
			StreamWriter writer;
			if( !File.Exists( newFilePath ) ) writer = File.CreateText( newFilePath );
			else writer = new StreamWriter( newFilePath );

			foreach( var line in modifiedLines )
			{
				writer.WriteLine( line );
			}
		}

		if( hadError )
		{
			Console.WriteLine( "Encountered error(s)! (Press enter to exit)" );
			Console.ReadLine();
		}
	}
}