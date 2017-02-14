﻿using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
	public class MyFile
	{
		public MyFolder Folder { get { return FileSystem.GetFolder(this); } }
		public FileSystem FileSystem { get; private set; }
		public string VirtualPath { get; private set; }
		public bool HasRealPath { get { return string.IsNullOrWhiteSpace(RealPath) == false; } }
		public string RealPath { get; private set; }

		FileChangedWatcher fileWatcher;
		event Action onFileChanged;

		public MyFile(FileSystem FileSystem, string virtualPath, string realPath)
		{
			this.FileSystem = FileSystem;
			this.VirtualPath = virtualPath;
			this.RealPath = realPath;
		}

		public Stream GetDataStream(int numOfRetries = 5)
		{
			while (numOfRetries >= 0)
			{
				try
				{
					return new FileStream(RealPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				catch (Exception e)
				{
					if (numOfRetries == 0) throw e;
					System.Threading.Thread.Sleep(10);
					numOfRetries--;
				}
			}
			throw new NotSupportedException();
		}

		public string ReadAllText(int numOfRetries = 5)
		{
			while (numOfRetries >= 0)
			{
				try
				{
					using (var sr = new StreamReader(GetDataStream(numOfRetries), Encoding.Default))
						return sr.ReadToEnd();
				}
				catch (Exception e)
				{
					if (numOfRetries == 0) throw e;
					System.Threading.Thread.Sleep(numOfRetries);
					numOfRetries--;
				}
			}
			throw new NotSupportedException();
		}

		public void OnFileChanged(Action action)
		{
			if (fileWatcher == null)
			{
				fileWatcher = new FileChangedWatcher();
				fileWatcher.WatchFile(RealPath, (newFileName) => onFileChanged.Raise());
			}
			onFileChanged += action;
		}

		public override string ToString()
		{
			return VirtualPath;
		}
	}
}