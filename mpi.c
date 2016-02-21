#include <stddef.h>
#include <mpi.h>

#ifdef WINDOWS
#define DLLEXPORT __declspec(dllexport)
#else
#define DLLEXPORT 
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

DLLEXPORT MPI_Op GetMpiOpSum() { return MPI_SUM; }

DLLEXPORT MpiType GetCommWorld() { return MPI_COMM_WORLD; }

DLLEXPORT MpiType GetMpiInt() { return MPI_INT; }
DLLEXPORT MpiType GetMpiFloat() { return MPI_FLOAT; }
DLLEXPORT MpiType GetMpiDouble() { return MPI_DOUBLE; }
DLLEXPORT MpiType GetMpiDoubleComplex() { return MPI_C_DOUBLE_COMPLEX; }


DLLEXPORT int GetMaxProcessorName() { return MPI_MAX_PROCESSOR_NAME; }
DLLEXPORT int GetMpiAnySource() { return MPI_ANY_SOURCE; }


DLLEXPORT int GetErrorString(int errorcode, char *string, int *resultlen)
{
	return MPI_Error_string(errorcode, string, resultlen);
}

DLLEXPORT int Init()
{
	return MPI_Init(0, NULL);
}

DLLEXPORT int Barrier(MPI_Comm comm)
{
	return MPI_Barrier(comm);
}

DLLEXPORT int AllGatherV(void* sendbuf, int size, void* rbuf, int *recvcounts, int *displs)
{
	return MPI_Allgatherv(sendbuf, size, MPI_DOUBLE_COMPLEX, rbuf, recvcounts, displs, MPI_DOUBLE_COMPLEX, MPI_COMM_WORLD);
}

DLLEXPORT int Gather(void *sendbuf, int sendcount, void *recvbuf, int recvcount, int root, MPI_Comm comm)
{
	return MPI_Gather(sendbuf, sendcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, root, comm);
}

DLLEXPORT int GatherV(void* sendbuf, int size, void* rbuf, int *recvcounts, int *displs)
{
	return MPI_Gatherv(sendbuf, size, MPI_DOUBLE_COMPLEX, rbuf, recvcounts, displs, MPI_DOUBLE_COMPLEX, 0, MPI_COMM_WORLD);
}

DLLEXPORT int AllReduce(void *sendbuf, void *recvbuf, int count, MPI_Datatype datatype, MPI_Comm comm)
{
	return MPI_Allreduce(sendbuf, recvbuf, count, datatype, MPI_SUM, comm);
}

DLLEXPORT int Reduce(void *sendbuf, void *recvbuf, int count, MPI_Datatype datatype, MPI_Comm comm)
{
	return MPI_Reduce(sendbuf, recvbuf, count, datatype, MPI_SUM, 0, comm);
}

DLLEXPORT int GetProcessorName(char *name, int *resultlen)
{
	return MPI_Get_processor_name(name, resultlen);
}

DLLEXPORT int GetCommWorldRank(int* rank)
{
	return MPI_Comm_rank(MPI_COMM_WORLD, rank);
}

DLLEXPORT int GetCommWorldSize(int* size)
{
	return MPI_Comm_size(MPI_COMM_WORLD, size);
}

DLLEXPORT int CommCreate(MPI_Comm comm, MPI_Group group, MPI_Comm *newcomm)
{
	return MPI_Comm_create(comm, group, newcomm);
}

DLLEXPORT int GroupIncl(MPI_Group group, int n, int ranks[], MPI_Group *newgroup)
{
	return MPI_Group_incl(group, n, ranks, newgroup);
}

DLLEXPORT int CommGroup(MPI_Comm comm, MPI_Group *group)
{
	return MPI_Comm_group(comm, group);
}

DLLEXPORT int Bcast(void *buffer, int count, MPI_Datatype datatype, int root, MPI_Comm comm)
{
	return	MPI_Bcast(buffer, count, datatype, root, comm);
}

DLLEXPORT int Send(void* data, int count, MPI_Datatype datatype, int dest, int tag, MPI_Comm comm)
{
	return MPI_Send(data, count, datatype, dest, tag, comm);
}

DLLEXPORT int Recv(void *buf, int count, MPI_Datatype datatype, int source, int tag, MPI_Comm comm, int* actualSource)
{
	MPI_Status status;
	int err = MPI_Recv(buf, count, datatype, source, tag, comm, &status);
	(*actualSource) = status.MPI_SOURCE;
	return err;
}

DLLEXPORT int SendComplexMatrix(void* data, int nx, int ny, int destination, int tag)
{
	return MPI_Send(data, nx*ny, MPI_DOUBLE_COMPLEX, destination, tag, MPI_COMM_WORLD);
}

DLLEXPORT int AllToAllDoubleComplex(complex16 *sendbuf, int sendcount, void *recvbuf, int recvcount, MPI_Comm comm)
{
	return MPI_Alltoall(sendbuf, sendcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, comm);
}

DLLEXPORT int AllToAllDoubleComplexInPlace(void *recvbuf, int recvcount, MPI_Comm comm)
{
	return MPI_Alltoall(MPI_IN_PLACE, recvcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, comm);
}

DLLEXPORT void Finalize()
{
	MPI_Finalize();
}
