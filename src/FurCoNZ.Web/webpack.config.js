const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

console.log('NODE_ENV is ' + (process.env.NODE_ENV || 'production'));

module.exports = {
    mode: process.env.NODE_ENV || 'production',
    entry: './FrontEnd/scss/index.scss',
    output: {
        path: path.resolve(__dirname, 'wwwroot/css'),
        publicPath: 'css',
        filename: 'bundle.js',
        chunkFilename: '[name].bundle.js',
    },
    devtool: 'source-map',
    resolve: {
        alias: {
            '~': path.resolve(__dirname, 'node_modules'),
        },
    },
    plugins: [
        new MiniCssExtractPlugin({
            // Options similar to the same options in webpackOptions.output
            // both options are optional
            filename: 'bundle.css',
            chunkFilename: 'bundle.[id].css',
            sourceMap: true,
        }),
    ],
    module: {
        rules: [
            {
                test: /\.scss$/,
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader,
                        options: {
                            publicPath: (resourcePath, context) => {
                                // publicPath is the relative path of the resource to the context
                                // e.g. for ./css/admin/main.css the publicPath will be ../../
                                // while for ./css/main.css the publicPath will be ../
                                return path.relative(path.dirname(resourcePath), context) + '/';
                            },
                            sourceMap: true,
                        },
                    },
                    {
                        loader: 'css-loader',
                        options: {
                            sourceMap: true,
                        },
                    },

                    {
                        loader: 'sass-loader',
                        options: {
                            sourceMap: true,
                            outputStyle: 'compressed',
                        },
                    },
                ],
            },
        ],
    },
};