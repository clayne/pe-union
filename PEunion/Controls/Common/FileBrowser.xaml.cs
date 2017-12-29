﻿using BytecodeApi;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PEunion
{
	public partial class FileBrowser : ObservableUserControl
	{
		public static readonly DependencyProperty AllowMultipleProperty = DependencyProperty.Register(nameof(AllowMultiple), typeof(bool), typeof(FileBrowser), new PropertyMetadata(AllowMultipleProperty_Changed));
		public static readonly DependencyProperty AllowedExtensionsProperty = DependencyProperty.Register(nameof(AllowedExtensions), typeof(string), typeof(FileBrowser));
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(FileBrowser));
		public static readonly DependencyProperty IconImageSourceProperty = DependencyProperty.Register(nameof(IconImageSource), typeof(ImageSource), typeof(FileBrowser), new PropertyMetadata(Utility.GetImageResource("ImageDragDrop"), IconImageSourceProperty_Changed));
		public bool AllowMultiple
		{
			get => (bool)GetValue(AllowMultipleProperty);
			set => SetValue(AllowMultipleProperty, value);
		}
		public string AllowedExtensions
		{
			get => (string)GetValue(AllowedExtensionsProperty);
			set => SetValue(AllowedExtensionsProperty, value);
		}
		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
		public ImageSource IconImageSource
		{
			get => (ImageSource)GetValue(IconImageSourceProperty);
			set => SetValue(IconImageSourceProperty, value);
		}
		private string[] AllowedExtensionsArray => AllowedExtensions.ToNullIfEmpty()?.Split(';');
		public string DragDropText => AllowMultiple ? "Drag one or multiple files here" : "Drag a file here";
		public event EventHandler<string[]> FilesSelect;

		public FileBrowser()
		{
			InitializeComponent();
		}

		private void UserControl_PreviewDragOver(object sender, DragEventArgs e)
		{
			e.Effects = CheckFiles(e.Data.GetData(DataFormats.FileDrop) as string[]) ? DragDropEffects.Copy : DragDropEffects.None;
			e.Handled = true;
		}
		private void UserControl_Drop(object sender, DragEventArgs e)
		{
			OnFilesSelect(e.Data.GetData(DataFormats.FileDrop) as string[]);
		}
		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			OnFilesSelect(AllowMultiple ? Dialogs.OpenMultiple(null, AllowedExtensionsArray) : Dialogs.Open(AllowedExtensionsArray).CreateSingletonArray());
		}

		private bool CheckFiles(string[] files)
		{
			return
				files?.Length > 0 &&
				files.All(File.Exists) &&
				(AllowMultiple || files.Length == 1) &&
				(AllowedExtensionsArray == null || files.All(file => AllowedExtensionsArray.Any(ext => PathEx.GetExtension(file).CompareCaseInsensitive(ext))));
		}
		private static void AllowMultipleProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FileBrowser control = d as FileBrowser;
			control.RaisePropertyChanged(() => control.DragDropText);
		}
		private static void IconImageSourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == null)
			{
				(d as FileBrowser).ClearValue(IconImageSourceProperty);
			}
		}
		protected void OnFilesSelect(string[] files)
		{
			if (CheckFiles(files)) FilesSelect?.Invoke(this, files);
		}
	}
}