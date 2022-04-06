// dentro do ciclo for, depois de pos, antes de chunkdata

using UnityEditor;
using UnityEngine;

Vector3 worldPos = chunk.transform.position + pos;

int h = Utils.GenerateHeight((int) worldPos.x, (int) worldPos.z);
int hs = Utils.GenerateStoneHeight((int) worldPos.x, (int) worldPos.z);

if (worldPos.y <= hs)
{
    if (Utils.FractionalBrownianMotion3D(worldPos, 1, 0.5f) < caveRatio) // 0.5f
    {
        chunkData -> STONE
    }
    else
    {
        chunkData -> AIR
    }
    
}
else if (worldPos.y == h)
{
    chunkData -> GRASS
}
else if (worldPos.y <= h)
{
    chunkData -> DIRT
}
else
{
    chunkData -> AIR
}

// apagar o rndom que segue



// add first person controller

// no fim de drawChunk() :
MeshCollider collider = chunk.AddComponent<MeshCollider>();
collider.sharedMesh = chunk.GetComponent<MeshFilter>().mesh;

/*
 * IDEIA GERAÇÃO:
 * PARA CADA XZ
 * PRIMEIRO, ESCOLHE SURFACE LEVEL
 * FAZ GRASS NO SITIO, STONE ABAIXO, AIR ACIMA
 * DEBAIXO DE SURFACE LEVEL (DENTRO DA CONDIÇÃO < H) ESCOLHE DIRT LEVEL, PINTA ACIMA
 * ?
*/

bool done = false;

// no fim de criar chunk, status = true

// World.cs : yield return null chunk a chunk (depois de add)
void BuildChunkAt(Vector3 chunkPos)
{
    string name = CreateChunkName(chunkPos);
    Chunk c;
    if (!regionData.TryGetValue(name, out c))
    {
        ...
            chunkDict Add
    }
}

void DrawChunks() // coroutine futuro
{
    foreach KeyValuePair, drawChunk
}

public GameObject player;
// alterar start
player.SetActive(false);
player.transform.position.y = Utils.GenerateHeight(player.transform.position.x, player.transform.position.z)+1;
lastBuildPos = GetPlayerChunkOrigin(player.transform.position);
BuildRecursiveWorld(lastBuildPos, renderDistance);
DrawChunks();
player.SetActive(true);

Vector3 GetPlayerChunkOrigin(Vector3 playerPos)
{
    Vector3 chunkPos = new Vector3();
    chunkPos.x = (int) (playerPos.x / chunkSize) * chunkSize;
    chunkPos.y = (int) (playerPos.y / chunkSize) * chunkSize;
    chunkPos.z = (int) (playerPos.z / chunkSize) * chunkSize;
    return chunkPos;
}

void RecursiveBuildWorld(Vector3 chunkPos, int distance)
{
    BuildChunkAt(chunkPos);
    if (--distance == 0)
    {
        return;
    }

    foreach (Vector3 dir in directions) relativo a player exceto back?
    {
        RecursiveBuildWorld(chunkPos+(dir*chunkSize), distance-1);
    }
}

void Update()
{
    Vector3 movement = player.transform.position - lastBuildPos;
    if (movement.magnitude > chunkSize)
    {
        BuildRecursiveWorld(lastBuildPos, renderDistance);
        DrawChunks();
    }
}