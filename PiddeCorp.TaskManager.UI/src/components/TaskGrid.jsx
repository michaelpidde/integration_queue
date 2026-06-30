import { useEffect, useRef , useState} from 'preact/hooks';
import { TabulatorFull as Tabulator } from 'tabulator-tables';

export function TaskGrid({ data }) {
    const tableRef = useRef(null);
    const tabulatorRef = useRef(null);
    const [isBuilt, setIsBuilt] = useState(false);

    useEffect(() => {
        tabulatorRef.current = new Tabulator(tableRef.current, {
            data: data,
            layout: 'fitColumns',
            columns: [
                { title: 'Id', field: 'id' },
                { title: 'Created', field: 'created' },
                { title: 'Type', field: 'typeName' },
                { title: 'Due', field: 'due' },
            ]
        });

        tabulatorRef.current.on('tableBuilt', () => {
            setIsBuilt(true);
        });

        return () => {
            tabulatorRef.current?.destroy();
            tabulatorRef.current = null;
            setIsBuilt(false);
        };
    }, []);

    // Update data when prop changes without reinitializing
    useEffect(() => {
        if (isBuilt && tabulatorRef.current && data) {
            tabulatorRef.current.setData(data);
        }
    }, [data, isBuilt]);

    return <div ref={tableRef} />;
}