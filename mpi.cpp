#include <stddef.h>

#include <iostream>
#include <mpi.h>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

#ifdef MSMPI
#define MpiType long long 
#endif

#ifdef OpenMPI
#define MpiType void*
#endif

#ifdef MPICH
#define MpiType long long
#endif

typedef struct { double re; double im; } complex16;

DllExport MPI_Op GetMpiOpSum() { return MPI_SUM; }
DllExport MpiType GetCommWorld() { return MPI_COMM_WORLD; }
DllExport MpiType GetMpiInt() { return MPI_INT; }
DllExport MpiType GetMpiFloat() { return MPI_FLOAT; }
DllExport MpiType GetMpiDouble() { return MPI_DOUBLE; }
DllExport MpiType GetMpiDoubleComplex() { return MPI_C_DOUBLE_COMPLEX; }


DllExport int GetMaxProcessorName() { return MPI_MAX_PROCESSOR_NAME; }
DllExport int GetMpiAnySource() { return MPI_ANY_SOURCE; }


DllExport int GetErrorString(int errorcode, char *string, int *resultlen)
{
	return MPI_Error_string(errorcode, string, resultlen);
}

DllExport int Init()
{
	return MPI_Init(0, NULL);
}

DllExport int Barrier(MPI_Comm comm)
{
	return MPI_Barrier(comm);
}

DllExport int AllGatherV(void* sendbuf, int size, void* rbuf, int *recvcounts, int *displs)
{
	return MPI_Allgatherv(sendbuf, size, MPI_DOUBLE_COMPLEX, rbuf, recvcounts, displs, MPI_DOUBLE_COMPLEX, MPI_COMM_WORLD);
}

DllExport int Gather(void *sendbuf, int sendcount, void *recvbuf, int recvcount, int root, MPI_Comm comm)
{
	return MPI_Gather(sendbuf, sendcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, root, comm);
}

DllExport int GatherV(void* sendbuf, int size, void* rbuf, int *recvcounts, int *displs)
{
	return MPI_Gatherv(sendbuf, size, MPI_DOUBLE_COMPLEX, rbuf, recvcounts, displs, MPI_DOUBLE_COMPLEX, 0, MPI_COMM_WORLD);
}

DllExport int AllReduce(void *sendbuf, void *recvbuf, int count, MPI_Datatype datatype, MPI_Comm comm)
{
	return MPI_Allreduce(sendbuf, recvbuf, count, MPI_DOUBLE_COMPLEX, MPI_SUM, comm);
}

DllExport int GetProcessorName(char *name, int *resultlen)
{
	return MPI_Get_processor_name(name, resultlen);
}

DllExport int GetCommWorldRank(int* rank)
{
	return MPI_Comm_rank(MPI_COMM_WORLD, rank);
}

DllExport int GetCommWorldSize(int* size)
{
	return MPI_Comm_size(MPI_COMM_WORLD, size);
}

DllExport int CommCreate(MPI_Comm comm, MPI_Group group, MPI_Comm *newcomm)
{
	return MPI_Comm_create(comm, group, newcomm);
}

DllExport int GroupIncl(MPI_Group group, int n, int ranks[], MPI_Group *newgroup)
{
	return MPI_Group_incl(group, n, ranks, newgroup);
}

DllExport int CommGroup(MPI_Comm comm, MPI_Group *group)
{
	return MPI_Comm_group(comm, group);
}

DllExport int Bcast(void *buffer, int count, MPI_Datatype datatype, int root, MPI_Comm comm)
{
	return	MPI_Bcast(buffer, count, datatype, root, comm);
}

DllExport int Send(void* data, int count, MPI_Datatype datatype, int dest, int tag, MPI_Comm comm)
{
	return MPI_Send(data, count, datatype, dest, tag, comm);
}

DllExport int Recv(void *buf, int count, MPI_Datatype datatype, int source, int tag, MPI_Comm comm, int* actualSource)
{
	MPI_Status status;
	int err = MPI_Recv(buf, count, datatype, source, tag, comm, &status);
	(*actualSource) = status.MPI_SOURCE;
	return err;
}

DllExport int SendComplexMatrix(void* data, int nx, int ny, int destination, int tag)
{
	return MPI_Send(data, nx*ny, MPI_DOUBLE_COMPLEX, destination, tag, MPI_COMM_WORLD);
}

DllExport int AllToAllDoubleComplex(complex16 *sendbuf, int sendcount, void *recvbuf, int recvcount, MPI_Comm comm)
{
	return MPI_Alltoall(sendbuf, sendcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, comm);
}

DllExport int AllToAllDoubleComplexInPlace(void *recvbuf, int recvcount, MPI_Comm comm)
{
	return MPI_Alltoall(MPI_IN_PLACE, recvcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, comm);
}

DllExport void Finalize()
{
	MPI_Finalize();
}
