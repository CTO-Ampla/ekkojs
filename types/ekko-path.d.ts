declare module 'ekko:path' {
    export const sep: string;
    export const delimiter: string;
    
    export function join(...paths: string[]): string;
    export function resolve(...paths: string[]): string;
    export function dirname(path: string): string;
    export function basename(path: string, ext?: string): string;
    export function extname(path: string): string;
    export function isAbsolute(path: string): boolean;
    export function relative(from: string, to: string): string;
    
    export interface ParsedPath {
        root: string;
        dir: string;
        base: string;
        ext: string;
        name: string;
    }
    
    export function parse(path: string): ParsedPath;
    export function format(pathObject: Partial<ParsedPath>): string;
    
    const path: {
        sep: string;
        delimiter: string;
        join: typeof join;
        resolve: typeof resolve;
        dirname: typeof dirname;
        basename: typeof basename;
        extname: typeof extname;
        parse: typeof parse;
        format: typeof format;
        isAbsolute: typeof isAbsolute;
        relative: typeof relative;
    };
    
    export default path;
}