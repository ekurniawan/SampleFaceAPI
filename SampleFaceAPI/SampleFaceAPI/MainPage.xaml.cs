using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;


namespace SampleFaceAPI
{
	public partial class MainPage : ContentPage
	{
        //menggunakan lib cognitive services
        private readonly IFaceServiceClient _faceServiceClient;
        ObservableCollection<Face> list = new ObservableCollection<Face>();

        public MainPage()
        {
            InitializeComponent();

            _faceServiceClient = new FaceServiceClient("1f940d1adfeb4d70992e9284588c76fe",
                "https://southeastasia.api.cognitive.microsoft.com/face/v1.0");

            list = new ObservableCollection<Face>();
            FacesListView.ItemsSource = list;

        }

        private async void TakePicture(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
            {
                await DisplayAlert("Ops", "Gagal mengambil gambar", "OK");

                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(
                new StoreCameraMediaOptions
                {
                    SaveToAlbum = false,
                    Directory = "Demo"
                });

            if (file == null)
                return;

            await FaceDetect(file.AlbumPath);

            MinhaImagem.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();


                return stream;

            });
        }

        public async Task FaceDetect(string image)
        {
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
                    FaceAttributeType.Gender,
                    FaceAttributeType.Age,
                    FaceAttributeType.Smile,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses,
                };

            list.Clear();

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(image))
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream,
                       returnFaceId: true,
                       returnFaceLandmarks: false,
                       returnFaceAttributes: faceAttributes);

                    //Add Faces in List
                    foreach (var face in faces)
                    {
                        list.Add(face);
                    }

                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                await DisplayAlert("Error", f.ErrorMessage, "ok");
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "ok");
            }
        }

    }
}
