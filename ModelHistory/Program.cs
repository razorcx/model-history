using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using History = Tekla.Structures.Model.History;

namespace ModelHistory
{
	public class Program
	{
		public static Model _model = new Model();
		public static Random _random = new Random();

		static void Main(string[] args)
		{
			var modelObjectEnums = new List<ModelObject.ModelObjectEnum>
			{
				ModelObject.ModelObjectEnum.BEAM
			};

			History.ModelHistory.TakeModifications("BEAMS", modelObjectEnums);

			ModelObjectEnumerator.AutoFetch = true;

			var beams = _model.GetModelObjectSelector()
				.GetAllObjectsWithType(ModelObject.ModelObjectEnum.BEAM)
				.ToAList<Beam>();


			//isModified
			var index = GetRandomIndex(0, beams.Count - 1);
			beams[index].Name = "MODIFIED";
			beams[index].Modify();

			//IsAttributeChanged
			index = GetRandomIndex(0, beams.Count - 1);
			beams[index].SetUserProperty("Comment", "This is a comment");
			beams[index].Modify();

			//IsCreated
			var beam = new Beam()
			{
				StartPoint = new Point(0, 0, 0),
				EndPoint = new Point(3000, 0, 0),
				Profile = new Profile  { ProfileString = "W360X33" }
			};
			beam.Insert();

			_model.CommitChanges();

			GetModifications();
		}

		private static int GetRandomIndex(int start, int end)
		{
			return _random.Next(start, end);
		}

		private static void GetModifications()
		{
			var modifications = History.ModelHistory
				.GetModifications("BEAMS")
				.Modified
				.ToAList<ModelObject>()
				.OfType<Part>()
				.ToList();

			var modificationsWithInfo = History.ModelHistory
				.GetModifications("BEAMS")
				.ModifiedWithInfo
				.ToList();

			modifications.ForEach(m =>
			{
				Console.WriteLine("Modified GUID: {0}", m.Identifier.GUID);
			});

			modificationsWithInfo.ForEach(m =>
			{
				Console.WriteLine("------------------------------");

				Console.WriteLine("Type {0}", m.ModelObject.GetType().Name);
				Console.WriteLine("Id: {0}", m.ModelObject.Identifier.GUID);
				Console.WriteLine("Is Attribute Changed: {0}", m.IsAttributeChanged);
				Console.WriteLine("Is Created: {0}", m.IsCreated);
				Console.WriteLine("Is Modified: {0}", m.IsModified);
			});

			Console.WriteLine("==============================");

			Console.ReadKey();
		}
	}

	public static class ExtensionMethods
	{
		public static List<T> ToAList<T>(this IEnumerator enumerator)
		{
			var list = new List<T>();
			while (enumerator.MoveNext())
			{
				var item = (T)enumerator.Current;
				if (item != null)
					list.Add(item);
			}
			return list;
		}
	}
}
