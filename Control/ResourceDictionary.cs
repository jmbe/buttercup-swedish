using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Resources;


namespace Buttercup.Control
{
	public class ResourceDictionary
	{
		#region Fields (1) 

		public static readonly DependencyProperty MergedDictionariesProperty = DependencyProperty.RegisterAttached(
				"MergedDictionaries", typeof(ResourceDictionary), typeof(ResourceDictionary),
				new PropertyMetadata(new PropertyChangedCallback(OnMergedDictionariesPropertyChanged)));

		#endregion Fields 



		#region Properties (2) 

		public string Keys { get; set; }

		[TypeConverter(typeof(UriTypeConverter))]
		public Uri Source { get; set; }

		#endregion Properties 



		#region Methods (5) 

		public static ResourceDictionary GetMergedDictionaries(DependencyObject d)
		{
			if(d == null)
			{
				throw new ArgumentNullException("d");
			}

			return (ResourceDictionary)d.GetValue(MergedDictionariesProperty);
		}



		public static void SetMergedDictionaries(DependencyObject d, ResourceDictionary dictionary)
		{
			if(d == null)
			{
				throw new ArgumentNullException("d");
			}

			d.SetValue(MergedDictionariesProperty, dictionary);
		}



		/// <summary>
		/// Gets the resource dictionary defined by the specified Source URI.
		/// </summary>
		/// <returns></returns>
		protected virtual System.Windows.ResourceDictionary GetResourceDictionary()
		{
			if(Source == null)
			{
				throw new Exception("Source property is not defined");
			}

			StreamResourceInfo resourceInfo = Application.GetResourceStream(Source);

			if(resourceInfo != null && resourceInfo.Stream != null)
			{
				using (StreamReader reader = new StreamReader(resourceInfo.Stream))
				{
					string xaml = reader.ReadToEnd();

					if(!string.IsNullOrEmpty(xaml))
					{
						return XamlReader.Load(xaml) as System.Windows.ResourceDictionary;
					}
				}
			}

			throw new Exception(string.Format("Resource dictionary '{0}' does not exist", Source));
		}



		/// <summary>
		/// (Instance-specific handler) Raised when the MergedDictionaries property is changed.
		/// Merges the keys and values from the current dictionary into the given target dictionary.
		/// </summary>
		/// <param name="targetDictionary">The target dictionary.</param>
		protected virtual void OnMergedDictionariesChanged(System.Windows.ResourceDictionary targetDictionary)
		{
			if(targetDictionary == null)
			{
				return;
			}
			if(string.IsNullOrEmpty(Keys))
			{
				throw new Exception("Keys property is not defined");
			}

			System.Windows.ResourceDictionary dictionaryToMerge = GetResourceDictionary();

			// NOTE: Silverlight 2 does not provide an enumerator or iteration option
			// for resource dictionaries

			foreach(string key in Keys.Split(",".ToCharArray()))
			{
				string kv = key.Trim();

				if(!string.IsNullOrEmpty(kv))
				{
					if(!dictionaryToMerge.Contains(kv))
					{
						throw new Exception(string.Format("Key '{0}' does not exist in resource dictionary '{1}'", kv, Source));
					}

					if(!targetDictionary.Contains(kv))
					{
						targetDictionary.Add(kv, dictionaryToMerge[kv]);
					}
				}
			}
		}



		/// <summary>
		/// Raised when the MergedDictionaries property is changed. This is a static handler that invokes
		/// the instance-specific handler.
		/// </summary>
		static void OnMergedDictionariesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ResourceDictionary dictionaryToMerge = e.NewValue as ResourceDictionary;

			if(d is System.Windows.ResourceDictionary)
			{
				dictionaryToMerge.OnMergedDictionariesChanged((d as System.Windows.ResourceDictionary));
			}
		}

		#endregion Methods 
	}
}