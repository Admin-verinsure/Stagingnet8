using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DealEngine.WebUI.Models
{
	public class CKEditorViewModel : BaseViewModel
	{
		public CKEditorViewModel()
        {
			IList<CKEditorBuild> DirectoryList = new List<CKEditorBuild>();
		}

		public IList<CKEditorBuild> DirectoryList { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string Placeholder { get; set; }

		public class CKEditorBuild
        {
			public string Name { get; set; }
			public string Path { get; set; }
			public string Placeholders { get; set; }
        }
	}
}

