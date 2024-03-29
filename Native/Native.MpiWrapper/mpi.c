//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
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

DLLEXPORT MpiType GetCommNull() { return MPI_COMM_NULL; }

DLLEXPORT MpiType GetMpiInt() { return MPI_INT; }

DLLEXPORT MpiType GetMpiFloat() { return MPI_FLOAT; }
DLLEXPORT MpiType GetMpiDouble() { return MPI_DOUBLE; }
DLLEXPORT MpiType GetMpiDoubleComplex() { return MPI_C_DOUBLE_COMPLEX; }


DLLEXPORT int GetMaxProcessorName() { return MPI_MAX_PROCESSOR_NAME; }
DLLEXPORT MpiType GetMpiAnySource() { return MPI_ANY_SOURCE; }
DLLEXPORT void* GetMpiInPlace(){ return MPI_IN_PLACE; }

DLLEXPORT int GetErrorString(int errorcode, char *string, int *resultlen)
{
	return MPI_Error_string(errorcode, string, resultlen);
}
DLLEXPORT void GetThreadSupportLevels(int* sup_level) { 
	sup_level[0]=MPI_THREAD_SINGLE;
	sup_level[1]=MPI_THREAD_FUNNELED;
	sup_level[2]=MPI_THREAD_SERIALIZED;
	sup_level[3]=MPI_THREAD_MULTIPLE;
  }

DLLEXPORT int Init(int* thread_support)
{
	int provided;
	int err;
	err=MPI_Init_thread(NULL, NULL,   MPI_THREAD_MULTIPLE, thread_support);

	return err;
}

DLLEXPORT long long CommunicatorC2Fortran(MPI_Comm comm)
{
	return  MPI_Comm_c2f(comm);
}

DLLEXPORT int CommSplit(MPI_Comm comm, int color, int key, MPI_Comm *newcomm){
	return MPI_Comm_split(comm, color,  key,	newcomm);
    }

DLLEXPORT int CommDup(MPI_Comm comm, MPI_Comm *newcomm){
	return MPI_Comm_dup(comm,  newcomm);
}

DLLEXPORT int Barrier(MPI_Comm comm)
{
	return MPI_Barrier(comm);
}

DLLEXPORT int AllGatherV(void* sendbuf, int size, void* rbuf, int *recvcounts, int *displs, MPI_Comm comm)
{
	return MPI_Allgatherv(sendbuf, size, MPI_DOUBLE_COMPLEX, rbuf, recvcounts, displs, MPI_DOUBLE_COMPLEX, comm);
}

DLLEXPORT int Gather(void *sendbuf, int sendcount, void *recvbuf, int recvcount, int root, MPI_Comm comm)
{
	return MPI_Gather(sendbuf, sendcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, root, comm);
}

DLLEXPORT int GatherV(void* sendbuf, int size, void* rbuf, int *recvcounts, int *displs, MPI_Comm comm)
{
	return MPI_Gatherv(sendbuf, size, MPI_DOUBLE_COMPLEX, rbuf, recvcounts, displs, MPI_DOUBLE_COMPLEX, 0, comm);
}

DLLEXPORT int AllReduce(void *sendbuf, void *recvbuf, int count, MPI_Datatype datatype, MPI_Comm comm)
{
	return MPI_Allreduce(sendbuf, recvbuf, count, datatype, MPI_SUM, comm);
}

DLLEXPORT int Reduce(void *sendbuf, void *recvbuf, int count, MPI_Datatype datatype, MPI_Comm comm)
{
	return MPI_Reduce(sendbuf, recvbuf, count, datatype, MPI_SUM, 0, comm);
}

//-----------------------------------
void PositiveDoubleLogicalOr( double *in, double *inout, int *len, MPI_Datatype *dptr )
    {
        int i;
        double c;

    	for (i=0; i< *len; i++) {
    		c=in[i] < 0 ? inout[i] : in[i];
            inout[i] = c;
        }
    }

//-------------------------------
DLLEXPORT int LogicalReduce(void *sendbuf, void *recvbuf, int count, MPI_Comm comm)
{
	MPI_Op logical_reduce;
	int error;
	int commutative=1;
	MPI_Op_create( PositiveDoubleLogicalOr, commutative, &logical_reduce );
	
	error=MPI_Reduce(sendbuf, recvbuf, count, MPI_DOUBLE, logical_reduce, 0, comm);
	MPI_Op_free(&logical_reduce);
	return error; 
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



DLLEXPORT int GetCommRank(MPI_Comm comm, int* rank)
{
	return MPI_Comm_rank(comm, rank);
}

DLLEXPORT int GetCommSize(MPI_Comm comm,int* size)
{
	return MPI_Comm_size(comm, size);
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


DLLEXPORT int Scatter(void* sendbuf,void* recvbuf, int count, MPI_Datatype datatype, int root, MPI_Comm comm)
{
	return MPI_Scatter(sendbuf, count, datatype, recvbuf, count, datatype,  root, comm);
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

DLLEXPORT int SendComplexMatrix(void* data, int nx, int ny, int destination, int tag, MPI_Comm comm)
{
	return MPI_Send(data, nx*ny, MPI_DOUBLE_COMPLEX, destination, tag, comm);
}

DLLEXPORT int AllToAllDoubleComplex(complex16 *sendbuf, int sendcount, void *recvbuf, int recvcount, MPI_Comm comm)
{
	return MPI_Alltoall(sendbuf, sendcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, comm);
}

DLLEXPORT int AllToAllDoubleComplexInPlace(void *recvbuf, int recvcount, MPI_Comm comm)
{
	return MPI_Alltoall(MPI_IN_PLACE, recvcount, MPI_DOUBLE_COMPLEX, recvbuf, recvcount, MPI_DOUBLE_COMPLEX, comm);
}


DLLEXPORT int SendRecv(void* sendbuf,void* recvbuf ,int count,int source, int dest, int tag,MPI_Datatype datatype,  MPI_Comm comm)
{
	return  MPI_Sendrecv(sendbuf, count, MPI_INT, dest, tag, recvbuf, count, datatype,  source, tag,comm, MPI_STATUS_IGNORE);
}

DLLEXPORT int CommFree(MPI_Comm* comm){
	return  MPI_Comm_free(comm);
}

int MPI_Comm_free(
  MPI_Comm *comm
);


DLLEXPORT void Finalize()
{
	MPI_Finalize();
}

    
