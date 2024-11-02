﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Wpf.Command;
using Wpf.Models;
using Wpf.Views;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;

namespace Wpf.ViewModels
{
	public class ReceiptViewModel : INotifyPropertyChanged
	{
		private readonly HttpClient _httpClient;
		public ObservableCollection<Receipt> Receipts { get; set; }
		private Receipt? _selectedReceipt;

		public Receipt? SelectedReceipt
		{
			get => _selectedReceipt;
			set
			{
				_selectedReceipt = value;
				OnPropertyChanged(nameof(SelectedReceipt));
			}
		}

		public ICommand AddCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand UpdateCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }

		public ReceiptViewModel()
		{
			_httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5001/") };
			Receipts = new ObservableCollection<Receipt>();
			LoadReceiptsAsync();

			AddCommand = new RelayCommand(OpenAddReceiptWindow);
			DeleteCommand = new RelayCommand(DeleteReceipt, () => SelectedReceipt != null);
			UpdateCommand = new RelayCommand(OpenUpdateReceiptWindow, () => SelectedReceipt != null);
			SaveCommand = new RelayCommand(SaveReceipt);
			CancelCommand = new RelayCommand(Cancel);
		}

		private async void LoadReceiptsAsync()
		{
			var response = await _httpClient.GetStringAsync("https://localhost:5001/api/receipts");
			var receipts = JsonConvert.DeserializeObject<List<Receipt>>(response);
			foreach (var receipt in receipts)
			{
				Receipts.Add(receipt);
			}
		}

		private void Cancel()
		{
			CloseReceiptDetailsWindow();
		}

		private async void SaveReceipt()
		{
			if (SelectedReceipt != null)
			{
				var json = JsonConvert.SerializeObject(SelectedReceipt);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				if (SelectedReceipt.ReceiptId == 0) // New receipt
				{
					await _httpClient.PostAsync("api/receipts", content);
					Receipts.Add(SelectedReceipt);
				}
				else // Update existing receipt
				{
					await _httpClient.PutAsync($"api/receipts/{SelectedReceipt.ReceiptId}", content);
				}
			}
			CloseReceiptDetailsWindow();
			SelectedReceipt = null;
		}

		private void CloseReceiptDetailsWindow()
		{
			var window = Application.Current.Windows.OfType<ReceiptDetailsView>().FirstOrDefault();
			window?.Close();
		}

		private void OpenUpdateReceiptWindow()
		{
			OpenReceiptDetailsWindow();
		}

		private void OpenAddReceiptWindow()
		{
			SelectedReceipt = new Receipt();
			OpenReceiptDetailsWindow();
		}

		private void OpenReceiptDetailsWindow()
		{
			var receiptDetailsWindow = new ReceiptDetailsView
			{
				DataContext = this
			};
			receiptDetailsWindow.ShowDialog();
		}

		private async void DeleteReceipt()
		{
			if (SelectedReceipt != null)
			{
				await _httpClient.DeleteAsync($"api/receipts/{SelectedReceipt.ReceiptId}");
				Receipts.Remove(SelectedReceipt);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}