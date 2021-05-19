using Sandbox;
using System.Collections.Generic;

class CubeUtils
{
	const float TileSize = 0.25f;

	public class MeshData
	{
		public List<Vertex> verts = new List<Vertex>();
		public List<int> triangles = new List<int>();

		public void AddVertex( Vector3 pos, Vector2 uv, Vector3 normal, Vector3 tangent )
		{
			var vert = new Vertex( pos, normal, tangent, new Vector4( uv.x, uv.y, 0.0f, 0.0f ) );
			verts.Add( vert );
		}

		public void AddQuadTriangles()
		{
			triangles.Add( verts.Count - 4 );
			triangles.Add( verts.Count - 3 );
			triangles.Add( verts.Count - 2 );

			triangles.Add( verts.Count - 4 );
			triangles.Add( verts.Count - 2 );
			triangles.Add( verts.Count - 1 );
		}
	}

	public static void FaceDataUp( float x, float y, float z, float BlockSize, MeshData meshData )
	{
		var normal = Vector3.Up;
		var tangent = Vector3.Right;

		meshData.AddVertex(
			new Vector3( x - 0.0f, y + BlockSize, z + BlockSize ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddVertex(
			new Vector3( x + BlockSize, y + BlockSize, z + BlockSize ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y + TileSize ),
			normal, tangent
			);

		meshData.AddVertex(
			new Vector3( x + BlockSize, y + BlockSize, z ),
			new Vector2( TileSize * normal.x, TileSize * normal.y + TileSize ),
			normal, tangent
			);

		meshData.AddVertex(
			new Vector3( x, y + BlockSize, z ),
			new Vector2( TileSize * normal.x, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddQuadTriangles();
	}

	public static void FaceDataDown( float x, float y, float z, float BlockSize, MeshData meshData )
	{
		var normal = Vector3.Down;
		var tangent = Vector3.Right;

		meshData.AddVertex(
			new Vector3( x, y, z ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y, z ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y, z + BlockSize ),
			new Vector2( TileSize * normal.x, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y, z + BlockSize ),
			new Vector2( TileSize * normal.x, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddQuadTriangles();
	}

	public static void FaceDataNorth( float x, float y, float z, float BlockSize, MeshData meshData )
	{
		var normal = Vector3.Forward;
		var tangent = Vector3.Up;

		meshData.AddVertex(
			new Vector3( x + BlockSize, y, z + BlockSize ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y + BlockSize, z + BlockSize ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y + BlockSize, z + BlockSize ),
			new Vector2( TileSize * normal.x, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y, z + BlockSize ),
			new Vector2( TileSize * normal.x, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddQuadTriangles();
	}

	public static void FaceDataEast( float x, float y, float z, float BlockSize, MeshData meshData )
	{
		var normal = Vector3.Right;
		var tangent = Vector3.Up;

		meshData.AddVertex(
			new Vector3( x + BlockSize, y, z ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y + BlockSize, z ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y + BlockSize, z + BlockSize ),
			new Vector2( TileSize * normal.x, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y, z + BlockSize ),
			new Vector2( TileSize * normal.x, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddQuadTriangles();
	}

	public static void FaceDataSouth( float x, float y, float z, float BlockSize, MeshData meshData )
	{
		var normal = Vector3.Backward;
		var tangent = Vector3.Up;

		meshData.AddVertex(
			new Vector3( x, y, z ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y + BlockSize, z ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y + BlockSize, z ),
			new Vector2( TileSize * normal.x, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x + BlockSize, y, z ),
			new Vector2( TileSize * normal.x, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddQuadTriangles();
	}

	public static void FaceDataWest( float x, float y, float z, float BlockSize, MeshData meshData )
	{
		var normal = Vector3.Left;
		var tangent = Vector3.Up;

		meshData.AddVertex(
			new Vector3( x, y, z + BlockSize ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y + BlockSize, z + BlockSize ),
			new Vector2( TileSize * normal.x + TileSize, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y + BlockSize, z ),
			new Vector2( TileSize * normal.x, TileSize * normal.y + TileSize ),
			normal, tangent
			);
		meshData.AddVertex(
			new Vector3( x, y, z ),
			new Vector2( TileSize * normal.x, TileSize * normal.y ),
			normal, tangent
			);

		meshData.AddQuadTriangles();
	}

	public static void AddVoxelToMesh( float x, float y, float z, float blockSize, MeshData meshData )
	{
		CubeUtils.FaceDataUp( x, y, z, blockSize, meshData );
		CubeUtils.FaceDataDown( x, y, z, blockSize, meshData );
		CubeUtils.FaceDataNorth( x, y, z, blockSize, meshData );
		CubeUtils.FaceDataSouth( x, y, z, blockSize, meshData );
		CubeUtils.FaceDataEast( x, y, z, blockSize, meshData );
		CubeUtils.FaceDataWest( x, y, z, blockSize, meshData );
	}
}
