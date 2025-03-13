package Services;

import java.util.List;

public interface Crud<T> {

    boolean create(T obj) throws Exception;

    boolean update(T obj) throws Exception;

    void delete(int id) throws Exception;

    List<T> getAll() throws Exception;

    T getById(int id) throws Exception;

}